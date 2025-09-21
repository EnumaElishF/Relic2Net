using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BagWindow : UI_CustomWindowBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform itemRoot;
    public override void Init()
    {
        closeButton.onClick.AddListener(CloseButtonClick);

    }
    private void CloseButtonClick()
    {
        UISystem.Close<UI_LoginWindow>();
    }
    public void Show(BagData bagData)
    {

    }
}
