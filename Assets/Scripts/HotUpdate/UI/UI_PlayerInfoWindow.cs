using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerInfoWindow : UI_WindowBase
{
    [SerializeField] private Image hpBarFillImage;
    public void UpdateHP(float fillAmount)
    {
        hpBarFillImage.fillAmount = fillAmount;
    }
}
