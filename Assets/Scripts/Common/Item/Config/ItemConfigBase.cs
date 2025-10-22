using UnityEngine;
using JKFrame;
using System.Collections.Generic;
/// <summary>
/// 继承的ConfigBase是继承奥丁可序列化的SO做的，方便后续使用字典上的序列化
/// 因为Unity自带的序列化系统默认不支持 Dictionary<TKey, TValue> 类型的序列化
/// </summary>
public abstract class ItemConfigBase : ConfigBase
{
    public int price; //商品的价格放在公共情况里使用，服务端与客户端都需要
    public ItemCraftConfig craftConfig;
    protected ItemDataBase defaultData;
    public abstract ItemDataBase GetDefaultItemData();

    //对ItemConfig“可Unity序列化的变量”的宏会导致内存布局变化导致在序列化失败的情况，移除他
    //考虑能不能被客户端调用的安全问题，如果是安全的话，就不用定在服务端，我们把他解放 去掉#if !UNITY_SERVER || UNITY_EDITOR
    public string slotPrefabPath;
    public Sprite icon;
    public Dictionary<LanguageType, string> nameDic;
    public Dictionary<LanguageType, string> descriptionDic;
    public string GetName(LanguageType languageType)
    {
        return nameDic[languageType];
    }
    public virtual string GetDescription(LanguageType languageType)
    {
        return descriptionDic[languageType];
    }
    public abstract string GetType(LanguageType languageType);
}
/// <summary>
/// 合成配置
/// </summary>
public class ItemCraftConfig
{
    public Dictionary<string, int> itemDic = new Dictionary<string, int>();
}