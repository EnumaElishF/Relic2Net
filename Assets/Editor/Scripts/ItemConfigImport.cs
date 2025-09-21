using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ItemConfigImport
{
    [MenuItem("Project/导入物品表格", priority = 1)]
    public static void Import()
    {
        string excelPath = Application.dataPath + "/Config/Excel/物品配置.xlsx";
        FileInfo fileInfo = new FileInfo(excelPath);
        using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
        {
            for(int i = 1; i <= 3; i++) //三张Sheet 1:武器 2：消耗品 3:材料
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[i];
                int maxCol = worksheet.Cells.Columns; //Excel会自动拓展行，容易出现空行，我们手动获取最大行
                for (int x = 2; x < maxCol; x++) //1是表头，从2开始为数据
                {
                    string key = worksheet.Cells[x, 1].Text.Trim();
                    if (string.IsNullOrEmpty(key)) break; //空行就跳出了,到下一Sheet
                    string chineseName = worksheet.Cells[x, 2].Text.Trim();
                    string englishName = worksheet.Cells[x, 3].Text.Trim();
                    string chineseDescription = worksheet.Cells[x, 4].Text.Trim();
                    string englishDescription = worksheet.Cells[x, 5].Text.Trim();

                    if(i==1)//武器
                    {
                        float attackValue = float.Parse(worksheet.Cells[x, 6].Text.Trim());
                        string configPath = $"Assets/Config/Item/Weapon/{key}.asset";
                        string iconPath = $"Assets/Res/Icon/Weapon/{key}.png";
                        string prefab = $"Assets/Prefab/Weapon/{key}.prefab";
                        string slotPrefabPath = "UI_WeaponSlot";
                        WeaponConfig itemConfig = AssetDatabase.LoadAssetAtPath<WeaponConfig>(configPath);
                        bool isCreate = itemConfig == null;
                        if (isCreate) itemConfig = WeaponConfig.CreateInstance<WeaponConfig>();
                        SetConfigCommon(itemConfig, chineseName, englishName, chineseDescription, englishDescription, iconPath, slotPrefabPath);
                        itemConfig.attackValue = attackValue;
                        itemConfig.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
                        EditorUtility.SetDirty(itemConfig); //Dirty重保存一下
                        if (isCreate) AssetDatabase.CreateAsset(itemConfig, configPath);
                        else AssetDatabase.SaveAssetIfDirty(itemConfig);

                    }else if(i == 2) //消耗品
                    {
                        float HPRegeneration = float.Parse(worksheet.Cells[x, 6].Text.Trim()); //HP恢复量
                        string configPath = $"Assets/Config/Item/Consumable/{key}.asset";
                        string iconPath = $"Assets/Res/Icon/Consumable/{key}.png";
                        string slotPrefabPath = "UI_ConsumableSlot";
                        ConsumableConfig itemConfig = AssetDatabase.LoadAssetAtPath<ConsumableConfig>(configPath);
                        bool isCreate = itemConfig == null;
                        if (isCreate) itemConfig = ConsumableConfig.CreateInstance<ConsumableConfig>();
                        SetConfigCommon(itemConfig, chineseName, englishName, chineseDescription, englishDescription, iconPath, slotPrefabPath);
                        itemConfig.HPRegeneration = HPRegeneration;
                        EditorUtility.SetDirty(itemConfig); //Dirty重保存一下
                        if (isCreate) AssetDatabase.CreateAsset(itemConfig, configPath);
                        else AssetDatabase.SaveAssetIfDirty(itemConfig);
                    }else if (i == 3) //材料
                    {
                        string configPath = $"Assets/Config/Item/Material/{key}.asset";
                        string iconPath = $"Assets/Res/Icon/Material/{key}.png";
                        string slotPrefabPath = "UI_MaterialSlot";

                        MaterialConfig itemConfig = AssetDatabase.LoadAssetAtPath<MaterialConfig>(configPath);
                        bool isCreate = itemConfig == null;
                        if (isCreate) itemConfig = MaterialConfig.CreateInstance<MaterialConfig>();
                        SetConfigCommon(itemConfig, chineseName, englishName, chineseDescription, englishDescription, iconPath, slotPrefabPath);
                        EditorUtility.SetDirty(itemConfig);
                        if (isCreate) AssetDatabase.CreateAsset(itemConfig, configPath);
                        else AssetDatabase.SaveAssetIfDirty(itemConfig);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("完成物品表格Excel的转换");
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="itemConfig"></param>
    /// <param name="chineseName"></param>
    /// <param name="englishName"></param>
    /// <param name="chineseDescription"></param>
    /// <param name="englishDescription"></param>
    /// <param name="iconPath"></param>
    /// <param name="slotPrefabPath">slotPrefabPath就是Addressables的Key</param>
    private static void SetConfigCommon(ItemConfigBase itemConfig, string chineseName, string englishName, string chineseDescription, string englishDescription, string iconPath,string slotPrefabPath)
    {
        //编译器下倒是可以忽略性能问题，在这里直接new也挺方便的
        itemConfig.nameDic = new Dictionary<LanguageType, string>()
        {
            { LanguageType.SimplifiedChinese,chineseName},
            { LanguageType.English,englishName},
        };
        itemConfig.descriptionDic = new Dictionary<LanguageType, string>()
        {
            { LanguageType.SimplifiedChinese,chineseDescription},
            { LanguageType.English,englishDescription},
        };
        itemConfig.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        //有可能已经设置过专属预制体，则忽视。举例就是武器类型的话，如果在本地化配置上，写SlotPrefabPath是手动放入过的，那么再次本地化就不会被覆盖了
        if (string.IsNullOrEmpty(itemConfig.slotPrafabPath)) 
        {
            itemConfig.slotPrafabPath = slotPrefabPath;
        }
    }
 }


