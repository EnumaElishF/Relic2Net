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
        NewClient();
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
    [MenuItem("Project/Build/NewClient")]
    public static void NewClient()
    {
        Debug.Log("��ʼ�����ͻ���");

        //HybridCLR����׼��
        PrebuildCommand.GenerateAll();//���л�٢��GenerateAll
        GenerateDllBytesFile();//����dll�ı��ļ�

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
        //Addressables���Զ�����
        Debug.Log("��ɹ����ͻ���");
    }

    [MenuItem("Project/Build/UpdateClient")]
    public static void UpdateClient()
    {
        //UpdateClient�ǲ���Ҫ��NewClient����ȥ���²���exe�ģ�������Ҫ����Դȥ������һ��
        Debug.Log("��ʼ�����ͻ��˸��°�");
        //��٢����Ҫȥ�������У�������Ҫ����dll
        CompileDllCommand.CompileDllActiveBuildTarget();
        GenerateDllBytesFile();
        //Addressables���°�
        string path = ContentUpdateScript.GetContentStateDataPath(false);
        Debug.Log(path);
        AddressableAssetSettings addressableAssetSettings = AddressableAssetSettingsDefaultObject.Settings;
        //�������ݵĸ���: ģ����Update a Previous Build
        ContentUpdateScript.BuildContentUpdate(addressableAssetSettings, path);

        Debug.Log("��ɹ����ͻ��˸��°�");


    }


    [MenuItem("Project/Build/GenerateDllBytesFile")]
    public static void GenerateDllBytesFile()
    {
        Debug.Log("��ʼ����dll�ı��ļ�");

        string dllDirPath = System.Environment.CurrentDirectory + "\\" + SettingsUtil.GetHotUpdateDllsOutputDirByTarget(EditorUserBuildSettings.activeBuildTarget).Replace('/','\\');
        string aotDllDirPath = System.Environment.CurrentDirectory  + "\\Assets\\Scripts\\DllBytes\\Aot";
        string hotUpdateDllDirPath = System.Environment.CurrentDirectory  + "\\Assets\\Scripts\\DllBytes\\HotUpdate";

        //���и���
        foreach(var dllName in SettingsUtil.AOTAssemblyNames)
        {
            //AOT·��  ��׺��true��ʾ����ԭ�����ļ�
            File.Copy($"{dllDirPath}\\{dllName}.dll", $"{aotDllDirPath}\\{dllName}.dll.bytes",true);
        }
        //�õ�������Ԥ���ĳ��򼯣��������Ҫ�ȸ���
        foreach(var dllName in SettingsUtil.HotUpdateAssemblyNamesExcludePreserved)
        {
            //�ȸ�·����
            File.Copy($"{dllDirPath}\\{dllName}.dll", $"{hotUpdateDllDirPath}\\{dllName}.dll.bytes", true);
        }

        //ˢ���ļ���
        AssetDatabase.Refresh();

        Debug.Log("�������dll�ı��ļ�");

    }
    #region ����˲��Ժ�
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
    #endregion

}
