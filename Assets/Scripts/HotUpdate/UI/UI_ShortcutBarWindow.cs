using JKFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ShortcutBarWindow : UI_WindowBase
{
    [SerializeField] private Transform itemRoot;
    private UI_SlotBase[] slots = new UI_SlotBase[GlobalUtility.itemShortcutBarCount]; //物品快捷栏格子数量
    private KeyCode[] shortcutKeycode = new KeyCode[GlobalUtility.itemShortcutBarCount];
    private int usedWeaponIndex = -1; // 本地的索引，-1指的是玩家使用的武器并不在快捷栏中

    private string emptySlotPath => ClientUtility.emptySlotPath;

    public void Show(BagData bagData)
    {
        usedWeaponIndex = -1;
        for (int i = 0; i < GlobalUtility.itemShortcutBarCount; i++)
        {
            int bagIndex = bagData.shortcutBarIndex[i];
            UI_SlotBase slot;
            int keyCode = i + 1;
            if(bagIndex == bagData.usedWeaponIndex)
            {
                usedWeaponIndex = i;
            }
            if (bagIndex == -1) //空格子
            {
                slot = CreateEmptySlot(bagIndex, keyCode);
            }
            else
            {
                slot = CreateItemSlot(bagIndex, keyCode, bagData.itemList[bagIndex]);
            }
            slots[i] = slot;
        }
        UpdateWeaponUsedState(usedWeaponIndex,true);
    }
    /// <summary>
    /// 格子关闭的，做个回收，回收到对象池里
    /// </summary>
    public override void OnClose()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Destroy();
            slots[i] = null;
        }
    }
    /// <summary>
    /// 道具栏带有内容的格子的创建
    /// </summary>
    /// <param name="bagIndex">背包索引</param>
    /// <param name="keyCode">当前的按键</param>
    /// <param name="itemData"></param>
    /// <returns></returns>
    private UI_SlotBase CreateItemSlot(int bagIndex, int keyCode, ItemDataBase itemData)
    {
        ItemConfigBase config = ResSystem.LoadAsset<ItemConfigBase>(itemData.id);
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrafabPath, itemRoot);
        slot.Init(itemData, config, bagIndex, OnUseItem, OnInteriorDragItem);
        slot.SetShortKeyCode(keyCode);
        return slot;
    }
    private UI_SlotBase CreateEmptySlot(int bagIndex, int keyCode)
    {
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, itemRoot);
        slot.Init(null, null, bagIndex, null, null);
        slot.SetShortKeyCode(keyCode);
        return slot;
    }

    private void OnUseItem(int bagIndex)
    {
        PlayerManager.Instance.UseItem(bagIndex);
    }
    public void UpdateItemByBagIndex(int bagIndex,ItemDataBase newData)
    {
        int newWeaponIndex = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            UI_SlotBase slot = slots[i];
            if (slot == null) continue;
            if(slot.bagIndex == bagIndex)
            {
                if (slot.bagIndex == PlayerManager.Instance.UsedWeaponIndex) newWeaponIndex = i; //武器发生改变
                slot.Destroy();
                int keyCode = i + 1;
                if (newData != null) slot = CreateItemSlot(bagIndex, keyCode, newData);
                else slot = CreateEmptySlot(bagIndex, keyCode);
                slot.transform.SetSiblingIndex(i);
                slots[i] = slot;
                break;
            }
        }
        if(usedWeaponIndex!= newWeaponIndex)
        {
            UpdateWeaponUsedState(usedWeaponIndex, false);
            UpdateWeaponUsedState(newWeaponIndex, true);
            usedWeaponIndex = newWeaponIndex;
        }
    }
    /// <summary>
    /// 更新武器有效性
    /// </summary>
    private void UpdateWeaponUsedState(int index,bool state)
    {
        if (index < 0) return;
        UI_WeaponSlot slot = slots[index] as UI_WeaponSlot;
        if (slot != null) slot.SetUseState(state);
    }

    private void OnInteriorDragItem(UI_SlotBase base1, UI_SlotBase base2)
    {
        
    }
}
