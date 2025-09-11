using JKFrame;
using UnityEngine;
using UnityEngine.UI;
public class UI_MainMenuWindow : UI_WindowBase
{
    [SerializeField] private Button loginButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    public override void Init()
    {
        loginButton.onClick.AddListener(LoginButtonClick);
        registerButton.onClick.AddListener(RegisterButtonClick);
        settingsButton.onClick.AddListener(SettingsButtonClick);
        quitButton.onClick.AddListener(QuitButtonClick);
    }

    private void SettingsButtonClick()
    {
        UISystem.Show<UI_GameSettingsWindow>();
    }

    private void LoginButtonClick()
    {
        UISystem.Show<UI_LoginWindow>();

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
