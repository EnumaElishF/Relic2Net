using UnityEngine;

[CreateAssetMenu(menuName = "Config/Item/" + nameof(ConsumableConfig))]
public class ConsumableConfig : ItemConfigBase
{
    public float HPRegeneration; // HP回复量
    public int defaultCountInShop;

    //public override ItemDataBase GetDefaultItemData()
    //{
    //    if (defaultData == null) defaultData = new ConsumableData { id = name, count = defaultCountInShop };
    //    return defaultData;
    //}
    public override string GetType(LanguageType languageType)
    {
        return LocalizationSystem.GetContent<LocalizationStringData>("消耗品", languageType).content;
    }
    public override string GetDescription(LanguageType languageType)
    {
        return $"{LocalizationSystem.GetContent<LocalizationStringData>("生命值恢复", languageType).content}{HPRegeneration},{base.GetDescription(languageType)}";
    }


}
