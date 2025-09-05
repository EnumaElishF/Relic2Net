using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
/// <summary>
/// 过滤掉热更新相关的程序集->给服务端使用的
/// </summary>
public class ServerBuildFilterAssemblies : IFilterBuildAssemblies
{
    public int callbackOrder => 1;

    public static bool enable = false;//默认不启用

    public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
    {
        if (!enable) return assemblies;
        return assemblies.Where(ass =>
        {
            string assName = Path.GetFileNameWithoutExtension(ass);//找到程序集名称
            bool reserved = !ass.Contains("HotUpdate");
            if (!reserved)
            {
                Debug.Log($"ServerBuildFilterAssemblies: 过滤了{assName}程序集");
            }
            return true;
        
        }).ToArray();
    }
}
