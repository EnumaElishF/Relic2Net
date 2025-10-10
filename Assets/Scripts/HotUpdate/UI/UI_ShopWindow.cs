using JKFrame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShopWindow : UI_CustomWindowBase, IItemWindow
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform itemRoot;
    private string emptySlotPath => ClientUtility.emptySlotPath;
    private List<UI_SlotBase> slotList = new List<UI_SlotBase>();
    private MerchantConfig merchantConfig;
    public override void Init()
    {
        closeButton.onClick.AddListener(CloseButtonClick);
    }
    private void CloseButtonClick()
    {
        UISystem.Close<UI_ShopWindow>();
    }
    public override void OnClose()
    {
        base.OnClose();
        for (int i = 0; i < slotList.Count; i++)
        {
            slotList[i].Destroy();
        }
        slotList.Clear();
    }

    public void Show(MerchantConfig merchantConfig)
    {
        this.merchantConfig = merchantConfig;
        List<ItemConfigBase> items = merchantConfig.items;
        for (int i = 0; i < BagData.itemCount; i++)
        {
            if (i >= items.Count) slotList.Add(CreateEmptySlot(i)); //大于的情况是空的格子
            else
            {
                ItemDataBase itemData = items[i].GetDefaultItemData();
                slotList.Add(CreateItemSlot(i, itemData));
            }
        }
    }
    private UI_SlotBase CreateItemSlot(int index, ItemDataBase itemData)
    {
        ItemConfigBase config = ResSystem.LoadAsset<ItemConfigBase>(itemData.id);
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(config.slotPrefabPath, itemRoot);
        slot.Init(this, itemData, config, index, null, OnInteriorDragItem);
        return slot;
    }

    private UI_SlotBase CreateEmptySlot(int index)
    {
        UI_SlotBase slot = ResSystem.InstantiateGameObject<UI_SlotBase>(emptySlotPath, itemRoot);
        slot.Init(this, null, null, index, null, null);
        return slot;
    }
    // A一定来自自身的，B不一定
    private void OnInteriorDragItem(UI_SlotBase slotA, UI_SlotBase slotB)
    {

    }
}
