using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using JKFrame;
/// <summary>
/// ����
/// </summary>
public static class BuildMenuItems
{
    public const string rootFloderPath = "Builds";
    public const string serverFloderPath = "Server";
    public const string clientFloderPath = "Client";
    [MenuItem("Project/Build/All")]
    public static void All()
    {
        //������ȫ����ʱ��Ҫ�ȴ�������Server���ٴ���ͻ���
        //��Ϊ�ڴ�������л��л�ƽ̨��
        //���������������ô��BuildSetting��Ҫ��Build��һ���л�����Dedicated Server�ĳ�Windows�ȡ�����������һֱ�ڷ���ˡ�
        Server();
        Client();
    }
    [MenuItem("Project/Build/Server")]
    public static void Server()
    {

        Debug.Log("��ʼ���������");

        //����˲���Ҫ�ȸ��£�   ��ProjectSetting��HybridCLR���ã�ȡ����ѡ��HybridCLR�����Enable
        HybridCLR.Editor.SettingsUtil.Enable = false;

        List<string> sceneList = new List<string>(EditorSceneManager.sceneCountInBuildSettings);
        for (int i =  0;i < EditorSceneManager.sceneCountInBuildSettings;i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath != null && !scenePath.Contains("Client")) //�ų�Client
            {
                Debug.Log("��ӳ���:" + scenePath);
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

        HybridCLR.Editor.SettingsUtil.Enable = true;//�ر�HybridCLR


        Debug.Log("��ɹ��������");

    }
    [MenuItem("Project/Build/Client")]
    public static void Client()
    {
        Debug.Log("��ʼ�����ͻ���");
        List<string> sceneList = new List<string>(EditorSceneManager.sceneCountInBuildSettings);
        for (int i = 0; i < EditorSceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath != null && !scenePath.Contains("Server")) //�ų�Server
            {
                Debug.Log("��ӳ���:" + scenePath);
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
        Debug.Log("��ɹ����ͻ���");
    }

    public static bool editorServerTest;
    public const string editorServerTestSymbolString = "SERVER_EDITOR_TEST";
    [MenuItem("Project/TestServer")]
    public static void TestServer()
    {
        editorServerTest = !editorServerTest;
        //����Ԥ����ָ��   �Զ����SERVER_EDITOR_TEST���Ƴ�Ԥ����ָ��  SERVER_EDITOR_TEST
        if (editorServerTest) JKFrameSetting.AddScriptCompilationSymbol(editorServerTestSymbolString);
        else JKFrameSetting.RemoveScriptCompilationSymbol(editorServerTestSymbolString);
        Menu.SetChecked("Project/TestServer", editorServerTest);
    }

}
