using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ShortcutBarWindow : MonoBehaviour
{
    [SerializeField] private Transform itemRoot;
    private UI_SlotBase[] slots = new UI_SlotBase[GlobalUtility.itemShortcutBarCount]; //物品快捷栏格子数量
    private KeyCode[] shortcutKeycode = new KeyCode[GlobalUtility.itemShortcutBarCount];
    private int usedWeaponIndex = -1; // 本地的索引，-1指的是玩家使用的武器并不在快捷栏中

    private string emptySlotPath => ClientUtility.emptySlotPath;

    public void Show(BagData bagData)
    {
        
    }
    private UI_SlotBase CreateItemSlot(int index, int keyCode, ItemDataBase itemData)
    {
        ItemConfigBase config = ResSystem.LoadAsset<ItemConfigBase>(itemData.id);
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrafabPath, itemRoot);
        slot.Init(itemData, config, index, OnUseItem);
        slot.SetShortKeyCode(keyCode);
        return slot;
    }
    private UI_SlotBase CreateEmptySlot(int index, int keyCode)
    {
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, itemRoot);
        slot.Init(null, null, index, null);
        slot.SetShortKeyCode(keyCode);
        return slot;
    }

    private void OnUseItem(int bagIndex)
    {
        PlayerManager.Instance.UseItem(bagIndex);
    }
}
