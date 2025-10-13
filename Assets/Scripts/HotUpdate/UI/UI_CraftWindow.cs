using JKFrame;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UI_CraftWindow : UI_CustomWindowBase, IItemWindow
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform itemRoot;
    [SerializeField] private Transform targetItemRoot;
    [SerializeField] private Transform craftItemRoot;
    [SerializeField] private Button craftSubmitButton;

    public const int itemCount = 6;
    public const int craftItemCount = 4;
    private string emptySlotPath => ClientUtility.emptySlotPath;
    private List<UI_SlotBase> slotList = new List<UI_SlotBase>();
    private UI_SlotBase[] craftItems = new UI_SlotBase[craftItemCount];
    private CrafterConfig crafterConfig;
    private UI_SlotBase targetItemSlog;
    public override void Init()
    {
        closeButton.onClick.AddListener(CloseButtonClick);
        //craftSubmitButton.onClick.AddListener(SubmitButtonClick);
    }

    private void CloseButtonClick()
    {
        UISystem.Close<UI_CraftWindow>();
    }
    public override void OnClose()
    {
        base.OnClose();

    }
    public void Show(CrafterConfig crafterConfig)
    {
        this.crafterConfig = crafterConfig;
        List<ItemConfigBase> items = crafterConfig.items;
        for (int i = 0; i < itemCount; i++)
        {
            if (i >= items.Count) slotList.Add(CreateEmptySlot(i, itemRoot));
            else
            {
                ItemDataBase itemData = items[i].GetDefaultItemData();
                slotList.Add(CreateItemSlot(i, itemData, itemRoot));
            }
        }
        CreateDefaultCraft();
    }
    private void CreateDefaultCraft()
    {
        //销毁已有的
        if (targetItemSlog != null) GameObject.Destroy(targetItemSlog.gameObject);
        targetItemSlog = CreateEmptySlot(0, targetItemRoot);
        for(int i = 0; i < craftItems.Length; i++)
        {
            if (craftItems[i] != null)
            {
                GameObject.Destroy(craftItems[i].gameObject);
            }
            craftItems[i] = CreateEmptySlot(i, craftItemRoot);
        }
    }

    private UI_SlotBase CreateItemSlot(int index, ItemDataBase itemData, Transform root)
    {
        ItemConfigBase config = ResSystem.LoadAsset<ItemConfigBase>(itemData.id);
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrefabPath, root);
        slot.Init(this, itemData, config, index, null, null);
        return slot;
    }

    private UI_SlotBase CreateEmptySlot(int index, Transform root)
    {
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, root);
        slot.Init(this, null, null, index, null, null);
        return slot;
    }
}
