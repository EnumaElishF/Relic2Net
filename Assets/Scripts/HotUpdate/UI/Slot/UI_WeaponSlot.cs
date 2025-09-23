using System;
using UnityEngine;

public class UI_WeaponSlot : UI_SlotBase<WeaponData, WeaponConfig>
{
    [SerializeField] private GameObject usedImage;
    private bool usedState = false;
    public override void Init(ItemDataBase data, ItemConfigBase config, int index, Action<int> onUseAction)
    {
        base.Init(data, config, index, onUseAction);
        SetUseState(false);
    }

    public void SetUseState(bool used)
    {
        this.usedState = used;
        usedImage.SetActive(used);
    }

}
