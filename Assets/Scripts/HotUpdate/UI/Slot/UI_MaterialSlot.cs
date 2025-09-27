using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_MaterialSlot : UI_SlotBase<MaterialData, MaterialConfig>
{
    [SerializeField] private Text countText;

    //材料不能被使用，onUseAction需要是null
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
