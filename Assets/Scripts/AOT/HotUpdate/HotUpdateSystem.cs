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
        //加一个序列化Serializable，让他可以保存住数据
        public bool hotUpdateSucceed;
    }

    [SerializeField] private TextAsset[] aotDllAssets;
    [SerializeField] private string[] hotUpdateDllNames;
    [SerializeField] private string versionInfoAddressableKey;
    private Action<float> onPercentageForEachFile;
    private Action<bool> onEnd;
    public void StartHotUpdate(Action<float> onPercentageForEachFile, Action<bool> onEnd)
    {
        //赋值一下全局参数的值，作为给当前方法传参
        this.onPercentageForEachFile = onPercentageForEachFile;
        this.onEnd = onEnd;
        //开始更新
        StartCoroutine(DoUpdateAddressables());
    }
    private IEnumerator DoUpdateAddressables()
    {
        //确定上一次热更新的状态
        HotUpdateSystemState state = SaveSystem.LoadSetting<HotUpdateSystemState>();
        if(state ==null || !state.hotUpdateSucceed) //从来没有热更过 || 上次热更没有成功
        {
            Debug.Log("断点续传");
            string catalogPath = $"{Application.persistentDataPath}/com.unity.addressables";
            if (Directory.Exists(catalogPath)) Directory.Delete(catalogPath, true); //删掉上次热更的catalog文件夹，以做到，断点续传，进行重新检查下载
        }

        //初始化
        yield return Addressables.InitializeAsync();
        bool succeed = true;
        //检测目录更新：CheckForCatalogUpdates会找服务器上的目录，从而做到本地存档的目录catalog和服务器上目录catalog比较，不相等就需要热更
        AsyncOperationHandle<List<string>> checkForCatalogUpdatesHandle  = Addressables.CheckForCatalogUpdates(false);
        yield return checkForCatalogUpdatesHandle;
        if (checkForCatalogUpdatesHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            Debug.LogError($"CheckForCatalogUpdates失败:{checkForCatalogUpdatesHandle.OperationException.Message}");
        }
        else
        {
            //下载最新的目录
            if (checkForCatalogUpdatesHandle.Result.Count > 0)
            {
                //IResourceLocator定位器
                AsyncOperationHandle<List<IResourceLocator>> updateCatalogsHandle = Addressables.UpdateCatalogs(checkForCatalogUpdatesHandle.Result, false);
                yield return updateCatalogsHandle;
                if (updateCatalogsHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    succeed = false;
                    Debug.LogError($"CheckForCatalogUpdates失败:{updateCatalogsHandle.OperationException.Message}");
                }
                else
                {
                    Debug.Log("下载目录更新成功");

                    ShowLoadingWindow();

                    List<IResourceLocator> locators = updateCatalogsHandle.Result;
                    foreach(IResourceLocator locator in locators)
                    {
                        //下面全部的内容，完成了一次循环，也就是下载完成了：一个资源
                        AsyncOperationHandle<long> sizeHandle  =  Addressables.GetDownloadSizeAsync(locator.Keys);
                        yield return sizeHandle;
                        if(sizeHandle.Status!= AsyncOperationStatus.Succeeded)
                        {
                            succeed = false;
                            Debug.LogError($"GetDownloadSizeAsync失败:{sizeHandle.OperationException.Message}");
                        }
                        else
                        {
                           long downloadSize = sizeHandle.Result;
                            if (downloadSize > 0)
                            {
                                //实际的下载  : Addressables.MergeMode.None只能拿到第一个包，因为我们需要全部的包，所以需要用并集 Addressables.MergeMode.Union
                                //AsyncOperationHandle downloadDependenciesHanle = Addressables.DownloadDependenciesAsync(locator.Keys, Addressables.MergeMode.None, false);
                                AsyncOperationHandle downloadDependenciesHanle = Addressables.DownloadDependenciesAsync(locator.Keys, Addressables.MergeMode.Union, false);
                                //循环查看下载进度
                                while (!downloadDependenciesHanle.IsDone)
                                {
                                    if(downloadDependenciesHanle.Status == AsyncOperationStatus.Failed) //因为还需要考虑为None的状态，所以不用！=Success
                                    {
                                        succeed = false;
                                        Debug.LogError($"downloadDependenciesHanle失败:{downloadDependenciesHanle.OperationException.Message}");
                                        break;
                                    }
                                    //分发下载进度
                                    float percentage = downloadDependenciesHanle.PercentComplete;
                                    Debug.Log($"下载进度:{percentage}");
                                    onPercentageForEachFile?.Invoke(percentage);

                                    UpdateLoadingWindowProgress(downloadSize * percentage, downloadSize);

                                    yield return CoroutineTool.WaitForFrame();//框架协程工具：避免gc
                                }
                                if (downloadDependenciesHanle.Status == AsyncOperationStatus.Succeeded)
                                {
                                    Debug.Log($"下载进度完成:{locator.LocatorId}");
                                }
                                Addressables.Release(downloadDependenciesHanle);

                            }
                        }
                        Addressables.Release(sizeHandle);
                    }

                    CloseLoadingWindow();
                }
            }
            else Debug.Log("无需更新");
        }

        Addressables.Release(checkForCatalogUpdatesHandle);

        if (state == null) state = new HotUpdateSystemState();
        state.hotUpdateSucceed = succeed;
        SaveSystem.SaveSetting(state); //保存为设置的二进制或者json格式数据，作为热更是否完全成功的标志，从而决定是否需要进行断点续传 (框架的工具设置)

        if (succeed)
        {
            LoadHotUpdateDll();
            LoadMetaForAOTAssemblies();
        }

        //所有的事情都干完，才能回调
        onEnd?.Invoke(succeed);

    }
    /// <summary>
    /// 获取版本信息
    /// </summary>
    /// 
    private string GetVersionInfo()
    {
        //这里版本信息就 不做异步 了，必须要等，因为要拿到版本信息才能继续去工作(因为他文件也小感受不大）
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
            Debug.Log($"加载{hotUpdateDllNames[i]}程序集");
        }
    }
    /// <summary>
    /// 加载元数据
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
