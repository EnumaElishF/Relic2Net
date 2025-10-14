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
    private ItemConfigBase targetItemConfig;
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
        DestroySlots();
        DestroyCraftArea();

    }
    private void DestroySlots()
    {
        for (int i = 0; i < slotList.Count; i++)
        {
            slotList[i].Destroy();
        }
        slotList.Clear();
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
                slotList.Add(CreateItemSlot(i, itemData, itemRoot, OnItemClick));
            }
        }
        CreateDefaultCraftArea();
    }
    /// <summary>
    /// 创建默认合成区域
    /// </summary>
    private void CreateDefaultCraftArea()
    {
        //销毁已有的
        DestroyCraftArea();
        targetItemSlog = CreateEmptySlot(0, targetItemRoot);
        for (int i = 0; i < craftItems.Length; i++)
        {
            craftItems[i] = CreateEmptySlot(i, craftItemRoot);
        }
    }
    private void OnItemClick(PointerEventData.InputButton button, int dataIndex)
    {
        if (button != PointerEventData.InputButton.Left) return;
        ItemConfigBase itemConfig = crafterConfig.items[dataIndex];
        //创建合成区域
        CreateCraftArea(targetItemConfig);
    }

    private void CreateCraftArea(ItemConfigBase targetItem)
    {
        DestroyCraftArea();
        targetItemSlog = CreateItemSlot(0, targetItem.GetDefaultItemData(), targetItemRoot, null);
        //TODO 设置合成区域的格子状态与数量
        Dictionary<string, int> craftItemDic = targetItem.craftConfig.itemDic;
        int i = 0;
        foreach(KeyValuePair<string,int> item in craftItemDic)
        {
            ItemConfigBase itemConfig = ResSystem.LoadAsset<ItemConfigBase>(item.Key);
            UI_SlotBase slot = CreateItemSlot(i, itemConfig.GetDefaultItemData(), craftItemRoot, null);
            //TODO 检验背包数量，当前是否满足这个条件
            craftItems[i] = slot;
            i += 1;
        }
        //合成材料所需数量
        for (; i < craftItemCount; i++)
        {
            craftItems[i] = CreateEmptySlot(i, craftItemRoot);
        }
    }

    private void DestroyTargetItemSlot()
    {
        targetItemSlog?.Destroy();
        targetItemSlog = null;
    }
    private void DestroyCraftArea()
    {
        DestroyTargetItemSlot();
        for (int i = 0; i < craftItems.Length; i++)
        {
            craftItems[i]?.Destroy();
            craftItems[i] = null;
        }
    }

    private UI_SlotBase CreateItemSlot(int index, ItemDataBase itemData, Transform root, Action<PointerEventData.InputButton, int> onClickAction)
    {
        ItemConfigBase config = ResSystem.LoadAsset<ItemConfigBase>(itemData.id);
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrefabPath, root);
        slot.Init(this, itemData, config, index, null, null, onClickAction);
        return slot;
    }

    private UI_SlotBase CreateEmptySlot(int index, Transform root)
    {
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, root);
        slot.Init(this, null, null, index, null, null);
        return slot;
    }
}
