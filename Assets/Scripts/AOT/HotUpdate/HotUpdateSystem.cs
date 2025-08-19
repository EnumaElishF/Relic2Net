using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using JKFrame;
using System;
using HybridCLR;

public class HotUpdateSystem : MonoBehaviour
{
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
    private IEnumerator DoUpdateAddressables()
    {
        //��ʼ��
        yield return Addressables.InitializeAsync();
        bool succeed = true;
        //���Ŀ¼����
        AsyncOperationHandle<List<string>> checkForCatalogUpdatesHandle  = Addressables.CheckForCatalogUpdates(false);
        yield return checkForCatalogUpdatesHandle;
        if (checkForCatalogUpdatesHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            Debug.LogError($"CheckForCatalogUpdatesʧ��:{checkForCatalogUpdatesHandle.OperationException.Message}");
        }
        else
        {
            //�������µ�Ŀ¼
            if (checkForCatalogUpdatesHandle.Result.Count > 0)
            {
                //IResourceLocator��λ��
                AsyncOperationHandle<List<IResourceLocator>> updateCatalogsHandle = Addressables.UpdateCatalogs(checkForCatalogUpdatesHandle.Result, false);
                yield return updateCatalogsHandle;
                if (updateCatalogsHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    succeed = false;
                    Debug.LogError($"CheckForCatalogUpdatesʧ��:{updateCatalogsHandle.OperationException.Message}");
                }
                else
                {
                    Debug.Log("����Ŀ¼���³ɹ�");
                    //�������ذ汾��Ϣ
                    TextAsset versionInfoTextAsset =  GetVersionInfo();
                    Debug.Log($"�汾��Ϣ:{versionInfoTextAsset.text}");


                    List<IResourceLocator> locators = updateCatalogsHandle.Result;
                    foreach(IResourceLocator locator in locators)
                    {
                        //����ȫ�������ݣ������һ��ѭ����Ҳ������������ˣ�һ����Դ
                        AsyncOperationHandle<long> sizeHandle  =  Addressables.GetDownloadSizeAsync(locator.Keys);
                        yield return sizeHandle;
                        if(sizeHandle.Status!= AsyncOperationStatus.Succeeded)
                        {
                            succeed = false;
                            Debug.LogError($"GetDownloadSizeAsyncʧ��:{sizeHandle.OperationException.Message}");
                        }
                        else
                        {
                           long downloadSize = sizeHandle.Result;
                            if (downloadSize > 0)
                            {
                                //ʵ�ʵ�����
                                AsyncOperationHandle downloadDependenciesHanle = Addressables.DownloadDependenciesAsync(locator.Keys, Addressables.MergeMode.None, false);
                                //ѭ���鿴���ؽ���
                                while (!downloadDependenciesHanle.IsDone)
                                {
                                    if(downloadDependenciesHanle.Status == AsyncOperationStatus.Failed) //��Ϊ����Ҫ����ΪNone��״̬�����Բ��ã�=Success
                                    {
                                        succeed = false;
                                        Debug.LogError($"downloadDependenciesHanleʧ��:{downloadDependenciesHanle.OperationException.Message}");
                                        break;
                                    }
                                    //�ַ����ؽ���
                                    float percentage = downloadDependenciesHanle.PercentComplete;
                                    Debug.Log($"���ؽ���:{percentage}");
                                    onPercentageForEachFile?.Invoke(percentage);
                                    yield return CoroutineTool.WaitForFrame();//���Э�̹��ߣ�����gc
                                }
                                if (downloadDependenciesHanle.Status == AsyncOperationStatus.Succeeded)
                                {
                                    Debug.Log($"���ؽ������:{locator.LocatorId}");
                                }
                                Addressables.Release(downloadDependenciesHanle);

                            }
                        }
                        Addressables.Release(sizeHandle);
                    }
                }
            }
            else Debug.Log("�������");
        }

        Addressables.Release(checkForCatalogUpdatesHandle);

        if (succeed)
        {
            LoadHotUpdateDll();
            LoadMetaForAOTAssemblies();
        }

        //���е����鶼���꣬���ܻص�
        onEnd?.Invoke(succeed);

    }
    /// <summary>
    /// ��ȡ�汾��Ϣ
    /// </summary>
    /// 
    private TextAsset GetVersionInfo()
    {
        //����汾��Ϣ�� �����첽 �ˣ�����Ҫ�ȣ���ΪҪ�õ��汾��Ϣ���ܼ���ȥ����(��Ϊ���ļ�ҲС���ܲ���
        Addressables.DownloadDependenciesAsync(versionInfoAddressableKey, true).WaitForCompletion();
        return Addressables.LoadAssetAsync<TextAsset>(versionInfoAddressableKey).WaitForCompletion();
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

}
