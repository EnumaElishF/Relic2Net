using JKFrame;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Config/CrafterConfig")]
public class CrafterConfig : ConfigBase
{
    public List<ItemConfigBase> items = new List<ItemConfigBase>();
}
