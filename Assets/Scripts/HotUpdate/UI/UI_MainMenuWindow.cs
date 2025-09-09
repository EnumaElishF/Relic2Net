using UnityEngine;
using JKFrame;
using UnityEngine.UI;
using System;
public class UI_MainMenuWindow : UI_WindowBase
{
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button quitButton;
    public override void Init()
    {
        loginButton.onClick.AddListener(LoginButtonClick);
        registerButton.onClick.AddListener(RegisterButtonClick);
        quitButton.onClick.AddListener(QuitButtonClick);
    }

    private void LoginButtonClick()
    {
        
    }
    private void RegisterButtonClick()
    {
        UISystem.Show<UI_RegisterWindow>();
    }
    private void QuitButtonClick()
    {
        Application.Quit();
    }
}
