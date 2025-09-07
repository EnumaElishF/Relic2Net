using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Config/VersionInfo")]
public class VersionInfo : ConfigBase
{
    public class VersionData
    {
        [Multiline]
        public string info;
    }
    public Dictionary<LanguageType, VersionData> versionInfoDic = new Dictionary<LanguageType, VersionData>();
    public VersionData GetVersionData(LanguageType languageType)
    {
        return versionInfoDic[languageType];
    }
}
