using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using JKFrame;
/// <summary>
/// 工具
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
        Client();
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
            target = BuildTarget.StandaloneWindows,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            locationPathName = $"{projectRootPath}/{rootFloderPath}/{serverFloderPath}/Server.exe"
        };
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        HybridCLR.Editor.SettingsUtil.Enable = true;//关闭HybridCLR


        Debug.Log("完成构建服务端");

    }
    [MenuItem("Project/Build/Client")]
    public static void Client()
    {
        Debug.Log("开始构建客户端");
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
            target = BuildTarget.StandaloneWindows,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            locationPathName = $"{projectRootPath}/{rootFloderPath}/{clientFloderPath}/Client.exe"
        };
        BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("完成构建客户端");
    }

    public static bool editorServerTest;
    public const string editorServerTestSymbolString = "SERVER_EDITOR_TEST";
    [MenuItem("Project/TestServer")]
    public static void TestServer()
    {
        editorServerTest = !editorServerTest;
        //增加预处理指令   自定义的SERVER_EDITOR_TEST，移除预处理指令  SERVER_EDITOR_TEST
        if (editorServerTest) JKFrameSetting.AddScriptCompilationSymbol(editorServerTestSymbolString);
        else JKFrameSetting.RemoveScriptCompilationSymbol(editorServerTestSymbolString);
        Menu.SetChecked("Project/TestServer", editorServerTest);
    }

}
