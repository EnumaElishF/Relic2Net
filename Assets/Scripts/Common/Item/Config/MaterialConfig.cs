using UnityEngine;

[CreateAssetMenu(menuName = "Config/Item/" + nameof(MaterialConfig))]
public class MaterialConfig : ItemConfigBase
{
    public int defaultCountInShop;

    public override ItemDataBase GetDefaultItemData()
    {
        if (defaultData == null) defaultData = new MaterialData { id = name, count = defaultCountInShop };
        return defaultData;
    }
    public override string GetType(LanguageType languageType)
    {
        return LocalizationSystem.GetContent<LocalizationStringData>("材料", languageType).content;
    }
}
