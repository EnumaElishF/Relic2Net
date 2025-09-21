using UnityEngine;
using UnityEngine.UI;

public class UI_MaterialSlot : UI_SlotBase<MaterialData, MaterialConfig>
{
    [SerializeField] private Text countText;
    public override void Init(ItemDataBase data, ItemConfigBase config)
    {
        base.Init(data,config);
        SetCount();
    }

    public void SetCount()
    {
        countText.text = itemData.count.ToString();
    }

}
