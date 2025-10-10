using JKFrame;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Config/MerchantConfig")]
public class MerchantConfig : ConfigBase
{
    public List<ItemConfigBase> items = new List<ItemConfigBase>();
}
