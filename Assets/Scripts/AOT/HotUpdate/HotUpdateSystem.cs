using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using JKFrame;
using System;
using HybridCLR;
using System.IO;

public class HotUpdateSystem : MonoBehaviour
{
    [Serializable]
    private class HotUpdateSystemState
    {
        //��һ�����л�Serializable���������Ա���ס����
        public bool hotUpdateSucceed;
    }

    [SerializeField] private TextAsset[] aotDllAssets;
    [SerializeField] private string[] hotUpdateDllNames;
    [SerializeField] private string versionInfoAddressableKey;
    private Action<float> onPercentageForEachFile;
    private Action<bool> onEnd;
    public void StartHotUpdate(Action<float> onPercentageForEachFile, Action<bool> onEnd)
    {
        //��ֵһ��ȫ�ֲ�����ֵ����Ϊ����ǰ��������
        this.onPercentageForEachFile = onPercentageForEachFile;
        this.onEnd = onEnd;
        //��ʼ����
        StartCoroutine(DoUpdateAddressables());
    }
    private bool succeed;
    private IEnumerator DoUpdateAddressables()
    {
        //ȷ����һ���ȸ��µ�״̬
        HotUpdateSystemState state = SaveSystem.LoadSetting<HotUpdateSystemState>();
        if(state ==null || !state.hotUpdateSucceed) //����û���ȸ��� || �ϴ��ȸ�û�гɹ�
        {
            Debug.Log("�ϵ�����");
            string catalogPath = $"{Application.persistentDataPath}/com.unity.addressables";
            if (Directory.Exists(catalogPath)) Directory.Delete(catalogPath, true); //ɾ���ϴ��ȸ���catalog�ļ��У����������ϵ��������������¼������
        }

        //Addressables ϵͳ�ĳ�ʼ�� :Addressables ���鱾���Ƿ���ڻ���Ŀ¼,�����ھ����´�����
        yield return Addressables.InitializeAsync();

        succeed = true;
        //���Ŀ¼���£�CheckForCatalogUpdates���ҷ������ϵ�Ŀ¼���Ӷ��������ش浵��Ŀ¼catalog�ͷ�������Ŀ¼catalog�Ƚϣ�����Ⱦ���Ҫ�ȸ�
        AsyncOperationHandle<List<string>> checkForCatalogUpdatesHandle  = Addressables.CheckForCatalogUpdates(false);
        yield return checkForCatalogUpdatesHandle;
        if (checkForCatalogUpdatesHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            Debug.LogError($"CheckForCatalogUpdatesʧ��:{checkForCatalogUpdatesHandle.OperationException.Message}");
            Addressables.Release(checkForCatalogUpdatesHandle);

        }
        else
        {
            List<string> catalogResult = checkForCatalogUpdatesHandle.Result;
            Addressables.Release(checkForCatalogUpdatesHandle);

            Debug.Log("�������µ�Ŀ¼: catalog");
            if (catalogResult.Count > 0)
            {
                ShowLoadingWindow();

                //IResourceLocator��λ��
                AsyncOperationHandle<List<IResourceLocator>> updateCatalogsHandle = Addressables.UpdateCatalogs(catalogResult, false);

                yield return updateCatalogsHandle;
                if (updateCatalogsHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    succeed = false;
                    Addressables.Release(updateCatalogsHandle);
                    Debug.LogError($"CheckForCatalogUpdatesʧ��:{updateCatalogsHandle.OperationException.Message}");
                }
                else
                {
                    List<IResourceLocator> locatorList = updateCatalogsHandle.Result;
                    Addressables.Release(updateCatalogsHandle);
                    JKLog.Log("����Ŀ¼���³ɹ�");
                    List<object> downloadKeys = new List<object>(1000);
                    foreach (IResourceLocator locator in locatorList)
                    {
                        downloadKeys.AddRange(locator.Keys);
                    }
                    //SetLoadingWindow();
                    yield return DownloadAllAssets(downloadKeys);
                    CloseLoadingWindow();
                }
            }
            else Debug.Log("�������");
        }


        if (state == null) state = new HotUpdateSystemState();
        state.hotUpdateSucceed = succeed;
        SaveSystem.SaveSetting(state); //����Ϊ���õĶ����ƻ���json��ʽ���ݣ���Ϊ�ȸ��Ƿ���ȫ�ɹ��ı�־���Ӷ������Ƿ���Ҫ���жϵ����� (��ܵĹ�������)

        if (succeed)
        {
            LoadHotUpdateDll();
            LoadMetaForAOTAssemblies();

            // ��ΪAddressables�ڳ�ʼ��Ŀ¼��ż���dll����ᵼ��AD����Ϊ����Ϊ�ȸ������е�������δ֪����Դ ��Ϊ��System.Object
            //Addressables.LoadContentCatalogAsync($"{Addressables.RuntimePath}/catalog.json");
        }

        //���е����鶼���꣬���ܻص�
        onEnd?.Invoke(succeed);

    }

    /// <summary>
    /// ����������Դ
    /// </summary>
    /// <returns></returns>
    private IEnumerator DownloadAllAssets(List<object> keys)
    {
        //����ȫ�������ݣ������һ��ѭ����Ҳ������������ˣ�һ����Դ
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync((IEnumerable<object>)keys);
        yield return sizeHandle;
        if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            Debug.LogError($"GetDownloadSizeAsyncʧ��:{sizeHandle.OperationException.Message}");
        }
        else
        {
            long downloadSize = sizeHandle.Result;
            if (downloadSize > 0)
            {
                //ʵ�ʵ�����  : Addressables.MergeMode.Noneֻ���õ���һ��������Ϊ������Ҫȫ���İ���������Ҫ�ò��� Addressables.MergeMode.Union
                AsyncOperationHandle downloadDependenciesHandle = Addressables.DownloadDependenciesAsync((IEnumerable<object>)keys, Addressables.MergeMode.Union, false);

                //ѭ���鿴���ؽ���
                while (!downloadDependenciesHandle.IsDone)
                {
                    if (downloadDependenciesHandle.Status == AsyncOperationStatus.Failed) //��Ϊ����Ҫ����ΪNone��״̬�����Բ��ã�=Success
                    {
                        succeed = false;
                        Debug.LogError($"downloadDependenciesHanleʧ��:{downloadDependenciesHandle.OperationException.Message}");
                        break;
                    }
                    // �ַ����ؽ���
                    float percentage = downloadDependenciesHandle.GetDownloadStatus().Percent;
                    onPercentageForEachFile?.Invoke(percentage);
                    UpdateLoadingWindowProgress(downloadSize * percentage, downloadSize);

                    yield return CoroutineTool.WaitForFrame();
                }
                if (downloadDependenciesHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    JKLog.Log($"ȫ���������");
                }
                Addressables.Release(downloadDependenciesHandle);
            }
        }
        Addressables.Release(sizeHandle);
    }


    /// <summary>
    /// ��ȡ�汾��Ϣ
    /// </summary>
    /// 
    private string GetVersionInfo()
    {
        //����汾��Ϣ�� �����첽 �ˣ�����Ҫ�ȣ���ΪҪ�õ��汾��Ϣ���ܼ���ȥ����(��Ϊ���ļ�ҲС���ܲ���
        Addressables.DownloadDependenciesAsync(versionInfoAddressableKey, true).WaitForCompletion();
        TextAsset textAsset =  Addressables.LoadAssetAsync<TextAsset>(versionInfoAddressableKey).WaitForCompletion();
        string info = textAsset.text;
        Addressables.Release(textAsset);
        return info;
    }

    private void LoadHotUpdateDll()
    {
        for(int i = 0; i < hotUpdateDllNames.Length; i++)
        {
            TextAsset dllTextAsset = Addressables.LoadAssetAsync<TextAsset>(hotUpdateDllNames[i]).WaitForCompletion();
            System.Reflection.Assembly.Load(dllTextAsset.bytes);
            Debug.Log($"����{hotUpdateDllNames[i]}����");
        }
    }
    /// <summary>
    /// ����Ԫ����
    /// </summary>
    private void LoadMetaForAOTAssemblies()
    {
        for(int i = 0; i < aotDllAssets.Length; i++)
        {
            byte[] dllBytes = aotDllAssets[i].bytes;
            LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
            Debug.Log($"LoadMetaForAOTAssemblies:{aotDllAssets[i].name},{errorCode}");
        }
    }

    private UI_LoadingWindow loadingWindow;
    private void ShowLoadingWindow()
    {
        loadingWindow = UISystem.Show<UI_LoadingWindow>();
        loadingWindow.Init(GetVersionInfo());
    }

    private void CloseLoadingWindow()
    {
        UISystem.Close<UI_LoadingWindow>();
        loadingWindow = null;
    }
    private void UpdateLoadingWindowProgress(float current, float max)
    {
        loadingWindow.UpdateDownloadProgress(current, max);
    }

}
