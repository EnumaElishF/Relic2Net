using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BagWindow : UI_CustomWindowBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform itemRoot;
    private string emptySlotPath = "UI_EmptySlot";
    private List<UI_SlotBase> slotList = new List<UI_SlotBase>();
    public override void Init()
    {
        closeButton.onClick.AddListener(CloseButtonClick);

    }
    public override void OnClose()
    {
        base.OnClose();
        for(int i = 0; i < slotList.Count; i++)
        {
            slotList[i].Destroy();
        }
        slotList.Clear();
    }
    private void CloseButtonClick()
    {
        UISystem.Close<UI_BagWindow>();
    }
    public void Show(BagData bagData)
    {
        for(int i = 0; i < bagData.itemList.Count; i++)
        {
            ItemDataBase itemData = bagData.itemList[i];
            if(itemData != null)
            {
                ItemConfigBase config = ResSystem.LoadAsset<ItemConfigBase>(itemData.id);
                UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrafabPath, itemRoot);
                slot.Init(itemData, config);
                slotList.Add(slot);
            }
            else //空格子
            {
                UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, itemRoot);
                slot.Init();
                slotList.Add(slot);
            }
        }
    }
}
