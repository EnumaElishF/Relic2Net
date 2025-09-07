using OfficeOpenXml;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 本地化脚本配置的导入：Office操作的库->EPPlus
/// </summary>
public static class LocalizationConfigImport 
{
    [MenuItem("ProjectTool/导入全局本地化",priority = 0)]
    public static void Import()
    {
        //获取SO资源并清空当前设置
        string soPath = "Assets/Config/GlobalLocalizationConfig.asset";
        LocalizationConfig localizationConfig = AssetDatabase.LoadAssetAtPath<LocalizationConfig>(soPath);
        localizationConfig.config.Clear();

        string excelPath = Application.dataPath+"/Config/Excel/本地化全局配置.xlsx";
        FileInfo fileInfo = new FileInfo(excelPath);
        using(ExcelPackage excelPackage = new ExcelPackage(fileInfo))
        {
            //一个Excel里有多张表格(Sheet)，我们先获取第一张。因为是从sheet1开始，另外行也是从1开始
            ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[1];
            int maxCol = worksheet.Cells.Columns; //Excel会自动拓展行，容易出现空行，我们手动获取最大行
            // Key 中文 英文 
            for(int x = 2; x < maxCol; x++) //1是表头，从2开始为数据
            {
                string key = worksheet.Cells[x, 1].Text.Trim();
                if (string.IsNullOrEmpty(key)) break; //空行就跳出了
                string chinese = worksheet.Cells[x, 2].Text.Trim();
                string english = worksheet.Cells[x, 3].Text.Trim();
                //传输给SO资源
                localizationConfig.config.Add(key, new Dictionary<LanguageType, LocalizationDataBase> 
                {
                    {LanguageType.SimplifiedChinese, new LocalizationStringData{ content = chinese } },
                    {LanguageType.English, new LocalizationStringData{ content = english } },
                });
            }
        }
        //强制 Unity 识别对象的修改:‘脏 (dirty)’状态”，从而确保修改能被正确保存、刷新或触发相关编辑器逻辑。
        EditorUtility.SetDirty(localizationConfig);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("完成全局本地化Excel的转换");
    }
}
