using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemInfoPopupWindow : UI_WindowBase
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text typeText;
    [SerializeField] private Text priceText;
    //[SerializeField] private Text priceText;
    [SerializeField] private Text descriptionText;
    private RectTransform rectTransform => ((RectTransform)transform);
    public override void Init()
    {
        base.Init();
    }

    public void Show(Vector3 uiWorldPosition, float topOffset, ItemConfigBase itemConfig)
    {
        // 位置计算
        //详情弹窗锁在道具旁边，同时控制弹窗的位置不能超出W屏幕范围
        Vector2 windowSize = rectTransform.sizeDelta; //弹窗在屏幕上位置
        transform.position = uiWorldPosition;
        Vector3 uiPos = rectTransform.anchoredPosition;
        uiPos.y += topOffset;
        Vector2 canvasSize = ClientGlobal.canvasSize;
        //宽的范围 canvasSize.x / -2  左侧，往右拉+ windowSize.x / 2    canvasSize.x / 2 右侧的，往左拉-windowSize.x / 2
        Vector2 posXRange = new Vector2(canvasSize.x / -2 + windowSize.x / 2, canvasSize.x / 2 - windowSize.x / 2);
        //高的范围 
        Vector2 posYRange = new Vector2(canvasSize.y / -2 + windowSize.y / 2, canvasSize.y / 2 - windowSize.y / 2);
        uiPos.x = Mathf.Clamp(uiPos.x, posXRange.x, posXRange.y);
        uiPos.y = Mathf.Clamp(uiPos.y, posYRange.x, posYRange.y);
        rectTransform.anchoredPosition = uiPos;

        // 显示物品信息
        iconImage.sprite = itemConfig.icon;
        nameText.text = itemConfig.GetName(LocalizationSystem.LanguageType);
        typeText.text = itemConfig.GetType(LocalizationSystem.LanguageType);
        descriptionText.text = itemConfig.GetDescription(LocalizationSystem.LanguageType);
        priceText.text = itemConfig.price.ToString();
    }
}
