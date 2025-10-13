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
    public void Show()
    {

    }
    private UI_SlotBase CreateItemSlot(int index, ItemDataBase itemData, Transform root, Action<PointerEventData.InputButton, int> onClickAction)
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
