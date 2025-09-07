using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
/// <summary>
/// 过滤掉热更新相关的程序集->做成通用的
/// </summary>
public class BuildFilterAssemblies : IFilterBuildAssemblies
{
    public int callbackOrder => 1;

    public static bool serverMode = false;//默认false
    /// <summary>
    /// 手动剥离Server程序集
    /// </summary>
    /// <param name="buildOptions"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
    {
        if (serverMode)
        {
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);//找到程序集名称
                //Server的模式，过滤掉 HotUpdate程序集（这个其实也就是Client用的程序集）
                bool reserved = !ass.Contains("HotUpdate");
                if (!reserved)
                {
                    Debug.Log($"ServerBuildFilterAssemblies: 过滤了{assName}程序集");
                }
                return reserved;

            }).ToArray();
        }
        else
        {
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);
                //非Server模式，过滤掉Server程序集
                bool reserved = !ass.Contains("Server");
                if (!reserved)
                {
                    Debug.Log($"ServerBuildFilterAssemblies: 过滤了{assName}程序集");
                }
                return reserved;

            }).ToArray();
        }

    }
}
