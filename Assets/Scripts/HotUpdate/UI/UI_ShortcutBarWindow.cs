using JKFrame;
using System;
using UnityEngine;

public class UI_ShortcutBarWindow : UI_WindowBase,IItemWindow
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
            int bagIndex = bagData.shortcutBarIndexs[i];
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
        for(int i = 0;i< slots.Length; i++)
        {
            shortcutKeycode[i] = Enum.Parse<KeyCode>($"Alpha{i + 1}"); //快捷键填充键盘数字完毕
        }
    }
    private void Update()
    {
        if(PlayerManager.Instance.localPlayer != null && PlayerManager.Instance.playerControlEnable && Input.anyKeyDown)
        {
            for(int i = 0; i < shortcutKeycode.Length; i++)
            {
                if (Input.GetKeyDown(shortcutKeycode[i]))
                {
                    //玩家快捷键使用物品
                    slots[i].Use();
                }
            }
        }
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
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrefabPath, itemRoot);
        slot.Init(this, itemData, config, bagIndex, OnUseItem, OnInteriorDragItem);
        slot.SetShortcutKeyCode(keyCode);
        return slot;
    }
    private UI_SlotBase CreateEmptySlot(int bagIndex, int keyCode)
    {
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, itemRoot);
        slot.Init(this, null, null, bagIndex);
        slot.SetShortcutKeyCode(keyCode);
        return slot;
    }

    private void OnUseItem(int bagIndex)
    {
        PlayerManager.Instance.UseItem(bagIndex);
    }
    public void UpdateItemByBagIndex(int bagIndex,ItemDataBase newData)
    {
        int newUseWeaponIndex = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            UI_SlotBase slot = slots[i];
            if (slot == null) continue;
            if(slot.dataIndex == PlayerManager.Instance.UsedWeaponIndex)
            {
                newUseWeaponIndex = i;
            }
            if(slot.dataIndex == bagIndex)
            {
                slot.Destroy();
                int keyCode = i + 1;
                if (newData != null) slot = CreateItemSlot(bagIndex, keyCode, newData);
                else slot = CreateEmptySlot(bagIndex, keyCode);
                slot.transform.SetSiblingIndex(i);
                slots[i] = slot;
                break;
            }
        }
        //关闭可能存在的旧武器; -1代表着不在快捷栏的武器
        if (usedWeaponIndex != -1 && slots[usedWeaponIndex].dataIndex != PlayerManager.Instance.UsedWeaponIndex)
        {
            UpdateWeaponUsedState(usedWeaponIndex, false);
        }
        //开启可能存在的新武器
        if(newUseWeaponIndex != -1)
        {
            UpdateWeaponUsedState(newUseWeaponIndex, true);
        }
        usedWeaponIndex = newUseWeaponIndex;

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

    public void SetItem(int shortcutBarIndex, int bagIndex, BagData bagData)
    {
        if (slots[shortcutBarIndex] != null) slots[shortcutBarIndex].Destroy();
        UI_SlotBase slot;
        int keyCode = shortcutBarIndex + 1;
        if (bagIndex == -1 || bagData.itemList[bagIndex] == null) slot = CreateEmptySlot(bagIndex, keyCode);
        else slot = CreateItemSlot(bagIndex, keyCode, bagData.itemList[bagIndex]);
        slots[shortcutBarIndex] = slot;
        slot.transform.SetSiblingIndex(shortcutBarIndex);
        if (bagIndex == bagData.usedWeaponIndex)
        {
            usedWeaponIndex = shortcutBarIndex;
            UpdateWeaponUsedState(usedWeaponIndex, true);
        }
    }


    public int GetItemIndex(UI_SlotBase slotB)
    {
        //考虑到格子做的是固定数量背包，所以使用暴力搜索没有什么性能问题
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == slotB)
            {
                return i;
            }
        }
        return -1;
    }
    private void OnInteriorDragItem(UI_SlotBase slotA, UI_SlotBase slotB)
    {
        //快捷栏内格子互换
        if(slotB.ownerWindow == this)
        {
            int indexA = GetItemIndex(slotA);
            int indexB = GetItemIndex(slotB);
            NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_ShortcutBarSwapItem, new C_S_ShortcutBarSwapItem
            {
                shortcutBarIndexA = indexA,
                shortcutBarIndexB = indexB
            });
        }
        // 快捷栏中的格子移到背包中意味着取消这个快捷栏
        else if(slotB.ownerWindow is UI_BagWindow){
            int indexA = GetItemIndex(slotA);
            NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_ShortcutBarSetItem, new C_S_ShortcutBarSetItem
            {
                shortcutBarIndex = indexA,
                bagIndex = -1
            });
        }
    }
}
