using UnityEngine;
using JKFrame;
using System.Collections.Generic;
/// <summary>
/// 继承的ConfigBase是继承奥丁可序列化的SO做的，方便后续使用字典上的序列化
/// 因为Unity自带的序列化系统默认不支持 Dictionary<TKey, TValue> 类型的序列化
/// </summary>
public abstract class ItemConfigBase : ConfigBase
{
#if !UNITY_SERVER || UNITY_EDITOR
    public Sprite icon;
    public Dictionary<LanguageType, string> nameDic;
    public Dictionary<LanguageType, string> descriptionDic;
    public string GetName(LanguageType languageType)
    {
        return nameDic[languageType];
    }
    public string GetDescription(LanguageType languageType)
    {
        return descriptionDic[languageType];
    }
#endif
}
