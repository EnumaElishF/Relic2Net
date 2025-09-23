using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_MaterialSlot : UI_SlotBase<MaterialData, MaterialConfig>
{
    [SerializeField] private Text countText;
    public override void Init(ItemDataBase data, ItemConfigBase config, int index, Action<int> onUseAction)
    {
        //材料不能被使用，onUseAction需要是null
        base.Init(data, config, index, null);
        SetCount();
    }

    public void SetCount()
    {
        countText.text = itemData.count.ToString();
    }

}
