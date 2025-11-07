using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class MonsterConfigImport
{
    [MenuItem("Project/导入怪物表格", priority = 2)]
    public static void Import()
    {
        string excelPath = Application.dataPath + "/Config/Excel/怪物配置.xlsx";
        FileInfo fileInfo = new FileInfo(excelPath);
        using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
        {
            
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[1]; //分表Sheet，多个为i，单表做1
            int maxCol = worksheet.Cells.Columns; //Excel会自动拓展行，容易出现空行，我们手动获取最大行
            for (int x = 2; x < maxCol; x++) //1是表头，从2开始为数据
            {
                string key = worksheet.Cells[x, 1].Text.Trim();
                if (string.IsNullOrEmpty(key)) break; //空行就跳出了,到下一Sheet
                string configPath = $"Assets/Config/Monster/{key}.asset";

                MonsterConfig monsterConfig = AssetDatabase.LoadAssetAtPath<MonsterConfig>(configPath);
                bool isCreate = monsterConfig == null;
                if (isCreate) monsterConfig = MonsterConfig.CreateInstance<MonsterConfig>();

                monsterConfig.nameDic = new Dictionary<LanguageType, string>
                {
                    { LanguageType.SimplifiedChinese,worksheet.Cells[x,2].Text.Trim()},
                    { LanguageType.English,worksheet.Cells[x,3].Text.Trim()},
                };
                monsterConfig.maxHP = float.Parse(worksheet.Cells[x, 4].Text.Trim());
                monsterConfig.attackValue = float.Parse(worksheet.Cells[x, 5].Text.Trim());
                monsterConfig.maxIdleTime = float.Parse(worksheet.Cells[x, 6].Text.Trim());
                monsterConfig.maxPatrolTime = float.Parse(worksheet.Cells[x, 7].Text.Trim());

                // 保存更新（覆盖文件内容）
                EditorUtility.SetDirty(monsterConfig); //Dirty重保存一下
                if (isCreate) AssetDatabase.CreateAsset(monsterConfig, configPath);
                else AssetDatabase.SaveAssetIfDirty(monsterConfig);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("完成Monster的Excel转换");
        }
    }
}
