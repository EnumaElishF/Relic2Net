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
    private Action<float> onPercentageForEachFile;
    private Action<bool> onOver;
    public void StartHotUpdate(Action<float> onPercentageForEachFile, Action<bool> onOver)
    {
        //赋值一下全局参数的值，作为给当前方法传参
        this.onPercentageForEachFile = onPercentageForEachFile;
        this.onOver = onOver;
        //开始更新
        StartCoroutine(DoUpdateAddressables());
    }
    private IEnumerator DoUpdateAddressables()
    {
        //初始化
        yield return Addressables.InitializeAsync();
        bool succeed = true;
        //检测目录更新
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
                                //实际的下载
                                AsyncOperationHandle downloadDependenciesHanle = Addressables.DownloadDependenciesAsync(locator.Keys, Addressables.MergeMode.None, false);
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
                }
            }
            else Debug.Log("无需更新");
        }
        onOver?.Invoke(succeed);

        Addressables.Release(checkForCatalogUpdatesHandle);

        if (succeed)
        {
            LoadHotUpdateDll();
            LoadMetaForAOTAssemblies();
        }

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

}
