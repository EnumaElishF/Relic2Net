using UnityEngine;

[CreateAssetMenu(menuName = "Config/Item/" + nameof(WeaponConfig))]
public class WeaponConfig : ItemConfigBase
{
    public GameObject prefab;
    public float attackValue;

    //public override ItemDataBase GetDefaultItemData()
    //{
    //    if (defaultData == null) defaultData = new WeaponData { id = name };
    //    return defaultData;
    //}

    public override string GetType(LanguageType languageType)
    {
        return LocalizationSystem.GetContent<LocalizationStringData>("武器", languageType).content;
    }
    public override string GetDescription(LanguageType languageType)
    {
        return $"{LocalizationSystem.GetContent<LocalizationStringData>("攻击力", languageType).content}+{attackValue},{base.GetDescription(languageType)}";
    }
}
