using UnityEngine;
using UnityEngine.UI;

public class UI_ConsumableSlot : UI_SlotBase<ConsumableData, ConsumableConfig>
{
    [SerializeField] private Text countText;
    public override void OnInit()
    {
        base.OnInit();
        SetCount();
    }

    public void SetCount()
    {
        countText.text = itemData.count.ToString();
    }

}
