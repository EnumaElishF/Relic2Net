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
    private bool succeed;
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

        //Addressables 系统的初始化 :Addressables 会检查本地是否存在缓存目录,不存在就重新创建。
        yield return Addressables.InitializeAsync();

        succeed = true;
        //检测目录更新：CheckForCatalogUpdates会找服务器上的目录，从而做到本地存档的目录catalog和服务器上目录catalog比较，不相等就需要热更
        AsyncOperationHandle<List<string>> checkForCatalogUpdatesHandle  = Addressables.CheckForCatalogUpdates(false);
        yield return checkForCatalogUpdatesHandle;
        if (checkForCatalogUpdatesHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            Debug.LogError($"CheckForCatalogUpdates失败:{checkForCatalogUpdatesHandle.OperationException.Message}");
            Addressables.Release(checkForCatalogUpdatesHandle);

        }
        else
        {
            List<string> catalogResult = checkForCatalogUpdatesHandle.Result;
            Addressables.Release(checkForCatalogUpdatesHandle);

            Debug.Log("下载最新的目录: catalog");
            if (catalogResult.Count > 0)
            {
                ShowLoadingWindow();

                //IResourceLocator定位器
                AsyncOperationHandle<List<IResourceLocator>> updateCatalogsHandle = Addressables.UpdateCatalogs(catalogResult, false);

                yield return updateCatalogsHandle;
                if (updateCatalogsHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    succeed = false;
                    Addressables.Release(updateCatalogsHandle);
                    Debug.LogError($"CheckForCatalogUpdates失败:{updateCatalogsHandle.OperationException.Message}");
                }
                else
                {
                    List<IResourceLocator> locatorList = updateCatalogsHandle.Result;
                    Addressables.Release(updateCatalogsHandle);
                    JKLog.Log("下载目录更新成功");
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
            else Debug.Log("无需更新");
        }


        if (state == null) state = new HotUpdateSystemState();
        state.hotUpdateSucceed = succeed;
        SaveSystem.SaveSetting(state); //保存为设置的二进制或者json格式数据，作为热更是否完全成功的标志，从而决定是否需要进行断点续传 (框架的工具设置)

        if (succeed)
        {
            LoadHotUpdateDll();
            LoadMetaForAOTAssemblies();

            // 因为Addressables在初始化目录后才加载dll，这会导致AD会认为类型为热更程序集中的类型是未知的资源 认为是System.Object
            //Addressables.LoadContentCatalogAsync($"{Addressables.RuntimePath}/catalog.json");
        }

        //所有的事情都干完，才能回调
        onEnd?.Invoke(succeed);

    }

    /// <summary>
    /// 下载所有资源
    /// </summary>
    /// <returns></returns>
    private IEnumerator DownloadAllAssets(List<object> keys)
    {
        //下面全部的内容，完成了一次循环，也就是下载完成了：一个资源
        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync((IEnumerable<object>)keys);
        yield return sizeHandle;
        if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
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
                AsyncOperationHandle downloadDependenciesHandle = Addressables.DownloadDependenciesAsync((IEnumerable<object>)keys, Addressables.MergeMode.Union, false);

                //循环查看下载进度
                while (!downloadDependenciesHandle.IsDone)
                {
                    if (downloadDependenciesHandle.Status == AsyncOperationStatus.Failed) //因为还需要考虑为None的状态，所以不用！=Success
                    {
                        succeed = false;
                        Debug.LogError($"downloadDependenciesHanle失败:{downloadDependenciesHandle.OperationException.Message}");
                        break;
                    }
                    // 分发下载进度
                    float percentage = downloadDependenciesHandle.GetDownloadStatus().Percent;
                    onPercentageForEachFile?.Invoke(percentage);
                    UpdateLoadingWindowProgress(downloadSize * percentage, downloadSize);

                    yield return CoroutineTool.WaitForFrame();
                }
                if (downloadDependenciesHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    JKLog.Log($"全部下载完成");
                }
                Addressables.Release(downloadDependenciesHandle);
            }
        }
        Addressables.Release(sizeHandle);
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
