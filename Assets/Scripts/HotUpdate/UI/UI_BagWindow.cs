using JKFrame;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BagWindow : UI_CustomWindowBase,IItemWindow
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform itemRoot;
    private string emptySlotPath => ClientUtility.emptySlotPath;
    private List<UI_SlotBase> slotList = new List<UI_SlotBase>();
    private BagData bagData;
    private int usedWeaponIndex;
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
        bagData = null; //关了的时候做空，防无效的引用
    }
    private void CloseButtonClick()
    {
        UISystem.Close<UI_BagWindow>();
    }
    public void Show(BagData bagData)
    {
        this.bagData = bagData;
        for(int i = 0; i < bagData.itemList.Count; i++)
        {
            ItemDataBase itemData = bagData.itemList[i];
            if(itemData != null)
                slotList.Add(CreateItemSlot(i,itemData));
            else //空格子
                slotList.Add(CreateEmptySlot(i));
        }
        usedWeaponIndex = bagData.usedWeaponIndex;
        UI_WeaponSlot weaponSlot = (UI_WeaponSlot)slotList[usedWeaponIndex];
        weaponSlot.SetUseState(true);
    }
    private UI_SlotBase CreateItemSlot(int index, ItemDataBase itemData)
    {
        ItemConfigBase config = ResSystem.LoadAsset<ItemConfigBase>(itemData.id);
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrafabPath, itemRoot);
        slot.Init(this,itemData, config, index, OnUseItem,OnInteriorDragItem);
        return slot;
    }
    private UI_SlotBase CreateEmptySlot(int index)
    {
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, itemRoot);
        slot.Init(this,null, null, index, null,null);
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
        if (index == bagData.usedWeaponIndex) // 武器格子发生了变化
        {
            if (usedWeaponIndex != index)
            {
                UI_WeaponSlot oldWeaponSlot = slotList[usedWeaponIndex] as UI_WeaponSlot;
                if (oldWeaponSlot != null) oldWeaponSlot.SetUseState(false);
            }

            UI_WeaponSlot newWeaponSlot = (UI_WeaponSlot)slotList[index];
            newWeaponSlot.SetUseState(true);
            usedWeaponIndex = index;
        }
    }
    //A一定是来自自身的，B不一定
    private void OnInteriorDragItem(UI_SlotBase slotA, UI_SlotBase slotB)
    {
        // 内部交换
        if(slotB.ownerWindow == this)
        {
            NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_BagSwapItem, new C_S_BagSwapItem
            {
                itemIndexA = slotA.bagIndex,
                itemIndexB = slotB.bagIndex,
            });
        }
        //设置这个格子到快捷栏  背包->快捷栏
        else if(slotB.ownerWindow is UI_ShortcutBarWindow)
        {
            int shortcurBagIndex = UISystem.GetWindow<UI_ShortcutBarWindow>().GetItemIndex(slotB);
            if(shortcurBagIndex != -1)
            {
                NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_ShortcutBarSetItem, new C_S_ShortcutBarSetItem
                {
                    shortcutBarIndex = shortcurBagIndex,
                    bagIndex = slotA.bagIndex
                });
            }
        }
    }
}
