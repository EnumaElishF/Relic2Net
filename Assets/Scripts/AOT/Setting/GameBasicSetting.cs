using System;
using UnityEngine;

/// <summary>
/// 基础设置
/// </summary>
//[SerializeField] !!!!严重错误，不小心把这里写错，导致System的IO加载错误，存档无法正常读
[Serializable]
public class GameBasicSetting
{
    public LanguageType languageType;
}
