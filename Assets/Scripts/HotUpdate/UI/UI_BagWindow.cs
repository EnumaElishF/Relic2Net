using JKFrame;
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
                slotList.Add(CreateItemSlot(i,itemData));
            else //空格子
                slotList.Add(CreateEmptySlot(i));
        }
    }
    private UI_SlotBase CreateItemSlot(int index, ItemDataBase itemData)
    {
        ItemConfigBase config = ResSystem.LoadAsset<ItemConfigBase>(itemData.id);
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrafabPath, itemRoot);
        slot.Init(itemData, config, index, OnUseItem);
        return slot;
    }
    private UI_SlotBase CreateEmptySlot(int index)
    {
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, itemRoot);
        slot.Init(null, null, index, null);
        return slot;
    }

    private void OnUseItem(int slotIndex)
    {
        PlayerManager.Instance.UseItem(slotIndex);
    }
    public void UpdateItem(int index,ItemDataBase itemData)
    {
        slotList[index].Destroy(); // 回收掉格子
        UI_SlotBase newSlot;
        if (itemData != null) newSlot = CreateItemSlot(index, itemData);
        else newSlot = CreateEmptySlot(index);
        //设置他是父亲的子物体的第几个
        newSlot.transform.SetSiblingIndex(index);
        slotList[index] = newSlot;
    }
}
