using UnityEngine;
using JKFrame;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;

public class UI_MessagePopupWindow : UI_WindowBase
{
    [SerializeField] private Text messageText;
    [SerializeField] private Image bglineImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private new Animation animation;

    public void ShowMessageByLocalizationKey(string localizationKey,Color color)
    {
        //本地化
        string message = LocalizationSystem.GetContent<LocalizationStringData>(localizationKey, LocalizationSystem.LanguageType).content;
        ShowMessage(message, color);
    }
    [Button]
    public void ShowMessage(string message, Color color)
    {
        messageText.text = message;
        //Color color = error ? Color.red : Color.yellow; //error的话还是用红色吧
        messageText.color = color;
        bglineImage.color = color;
        iconImage.color = color;
        animation.Play("Popup");
    }
    #region 动画事件
    /// <summary>
    /// 关闭弹窗
    /// </summary>
    private void OnPopupEnd()
    {
        animation.Stop();
        UISystem.Close<UI_MessagePopupWindow>();
    }
    #endregion
}
