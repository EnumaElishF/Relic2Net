using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using JKFrame;
using HybridCLR.Editor;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using System.Data.SqlTypes;
using UnityFS;
using HybridCLR.Editor.Commands;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
/// <summary>
/// 打包工具
/// </summary>
public static class BuildMenuItems
{
    public const string rootFloderPath = "Builds";
    public const string serverFloderPath = "Server";
    public const string clientFloderPath = "Client";
    [MenuItem("Project/Build/All")]
    public static void All()
    {
        //点击打包全部的时候，要先打包服务端Server，再打包客户端
        //因为在打包过程中会切换平台。
        //如果服务端是最后，那么在BuildSetting还要把Build做一个切换，从Dedicated Server改成Windows等。否则会出问题一直在服务端。
        Server();
        NewClient();
    }
    [MenuItem("Project/Build/Server")]
    public static void Server()
    {

        Debug.Log("开始构建服务端");

        //服务端不需要热更新：   对ProjectSetting的HybridCLR设置，取消勾选，HybridCLR插件的Enable
        HybridCLR.Editor.SettingsUtil.Enable = false;

        List<string> sceneList = new List<string>(EditorSceneManager.sceneCountInBuildSettings);
        for (int i =  0;i < EditorSceneManager.sceneCountInBuildSettings;i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath != null && !scenePath.Contains("Client")) //排除Client
            {
                Debug.Log("添加场景:" + scenePath);
                sceneList.Add(scenePath);
            }
        }
        string projectRootPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = sceneList.ToArray(),
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            locationPathName = $"{projectRootPath}/{rootFloderPath}/{serverFloderPath}/Server.exe"
        };
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        HybridCLR.Editor.SettingsUtil.Enable = true;//关闭HybridCLR


        Debug.Log("完成构建服务端");

    }
    [MenuItem("Project/Build/NewClient")]
    public static void NewClient()
    {
        Debug.Log("开始构建客户端");

        //HybridCLR构建准备
        PrebuildCommand.GenerateAll();//进行华佗的GenerateAll
        GenerateDllBytesFile();//搬运dll文本文件

        List<string> sceneList = new List<string>(EditorSceneManager.sceneCountInBuildSettings);
        for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath != null && !scenePath.Contains("Server")) //排除Server
            {
                Debug.Log("添加场景:" + scenePath);
                sceneList.Add(scenePath);
            }
        }
        string projectRootPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = sceneList.ToArray(),
            target = BuildTarget.StandaloneWindows64,  //生成目标文件夹名称为StandaloneWindows64
            subtarget = (int)StandaloneBuildSubtarget.Player,
            locationPathName = $"{projectRootPath}/{rootFloderPath}/{clientFloderPath}/Client.exe"
        };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        //Addressables会自动构建
        Debug.Log("完成构建客户端");
    }

    [MenuItem("Project/Build/UpdateClient")]
    public static void UpdateClient()
    {
        //UpdateClient是不需要像NewClient那样去重新产生exe的；但是需要把资源去重新走一遍
        Debug.Log("开始构建客户端更新包");
        //华佗不需要去更新所有，但是需要更新dll
        CompileDllCommand.CompileDllActiveBuildTarget();
        GenerateDllBytesFile();
        //Addressables更新包
        string path = ContentUpdateScript.GetContentStateDataPath(false);
        Debug.Log(path);
        AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
        //构建内容的更新: 模拟点击Update a Previous Build
        ContentUpdateScript.BuildContentUpdate(addressableAssetSettings, path);

        Debug.Log("完成构建客户端更新包");


    }


    [MenuItem("Project/Build/GenerateDllBytesFile")]
    public static void GenerateDllBytesFile()
    {
        Debug.Log("开始生成dll文本文件");

        //新加 aot的更新  :也就是HybridCLR下的 AssembliesPostIl2CppStrip 的StandaloneWindows64文件夹内文件
        string aotUpdateDllDirPath = System.Environment.CurrentDirectory + "\\" + SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget).Replace('/','\\');
        //原 基本的热更新   :也就是HybridCLR下的 HotUpdateDlls 的StandaloneWindows64文件夹内文件
        string hotUpdateDllDirPath = System.Environment.CurrentDirectory + "\\" + SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget).Replace('/','\\');
        string aotDllTextDirPath = System.Environment.CurrentDirectory  + "\\Assets\\Scripts\\DllBytes\\Aot";
        string hotUpdateDllTextDirPath = System.Environment.CurrentDirectory  + "\\Assets\\Scripts\\DllBytes\\HotUpdate";

        //进行复制
        foreach(var dllName in SettingsUtil.AOTAssemblyNames)
        {
            string path = $"{aotUpdateDllDirPath}\\{dllName}.dll";
            if (File.Exists(path))
            {
                //优先放AOT的  （新加
                File.Copy(path, $"{aotDllTextDirPath}\\{dllName}.dll.bytes", true); //加入这个是为了更合理，避免一些，万一裁剪到的情况，
            }
            else
            {
                //然后热更新的
                path = $"{hotUpdateDllDirPath}\\{dllName}.dll";
                //AOT路径aotDllTextDirPath  后缀加true表示覆盖原已有文件
                File.Copy(path, $"{aotDllTextDirPath}\\{dllName}.dll.bytes", true);
            }

        }
        //拿到不包含预留的程序集，这就是需要热更的
        foreach(var dllName in SettingsUtil.HotUpdateAssemblyNamesExcludePreserved)
        {
            //热更路径下
            File.Copy($"{hotUpdateDllDirPath}\\{dllName}.dll", $"{hotUpdateDllTextDirPath}\\{dllName}.dll.bytes", true);
        }

        //刷新文件夹
        AssetDatabase.Refresh();

        Debug.Log("完成生成dll文本文件");

    }
    #region 服务端测试宏
    public static bool editorServerTest;
    public const string editorServerTestSymbolString = "UNITY_EDITOR";
    [MenuItem("Project/TestServer")]
    public static void TestServer()
    {
        editorServerTest = !editorServerTest;
        //增加预处理指令   自定义的UNITY_EDITOR，移除预处理指令  UNITY_EDITOR
        if (editorServerTest) JKFrameSetting.AddScriptCompilationSymbol(editorServerTestSymbolString);
        else JKFrameSetting.RemoveScriptCompilationSymbol(editorServerTestSymbolString);
        Menu.SetChecked("Project/TestServer", editorServerTest);
    }
    #endregion

}
