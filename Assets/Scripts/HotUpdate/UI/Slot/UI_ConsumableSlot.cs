using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConsumableSlot : UI_SlotBase<ConsumableData, ConsumableConfig>
{
    [SerializeField] private Text countText;
    public override void Init(ItemDataBase data, ItemConfigBase config, int index, Action<int> onUseAction)
    {
        base.Init(data, config, index, onUseAction);
        SetCount();
    }

    public void SetCount()
    {
        countText.text = itemData.count.ToString();
    }

}
