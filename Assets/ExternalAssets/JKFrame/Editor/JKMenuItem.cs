using JKFrame;
using UnityEditor;
using UnityEngine;

public class JKMenuItem
{
    [MenuItem("WorkFrame/打开存档路径")]
    public static void OpenArchivedDirPath()
    {
        string path = Application.persistentDataPath.Replace("/", "\\");
        System.Diagnostics.Process.Start("explorer.exe", path);
    }
    [MenuItem("WorkFrame/打开框架文档")]
    public static void OpenDoc()
    {
        Application.OpenURL("http://www.yfjoker.com/JKFrame/index.html");
    }
    [MenuItem("WorkFrame/清空存档")]
    public static void CleanSave()
    {
        SaveSystem.DeleteAll();
    }

#if ENABLE_ADDRESSABLES
    [MenuItem("WorkFrame/生成资源引用代码")]
    public static void GenerateResReferenceCode()
    {
        GenerateResReferenceCodeTool.GenerateResReferenceCode();
    }
    [MenuItem("WorkFrame/清理资源引用代码")]
    public static void CleanResReferenceCode()
    {
        GenerateResReferenceCodeTool.CleanResReferenceCode();
    }
#endif
}