using JKFrame;
using UnityEngine;
using UnityEngine.UI;
public class UI_MainMenuWindow : UI_CustomWindowBase
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
    /// <summary>
    /// 之前的返回主菜单采用的是不断开网络连接，只是删除角色，但是这样会导致NetCode内部有一些垃圾数据并没有清理而导致同步问题（不同步某个角色、一直报错等），为了避免这种风险，所以
    /// 修改为返回主菜单直接断开连接，然后再重连来重置一次全部数据
    /// </summary>
    private void LoginButtonClick()
    {
        if (NetManager.Instance.IsConnectedClient || NetManager.Instance.InitClient())
        {
            UISystem.Show<UI_LoginWindow>();
        }
        else
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessage("Net error",Color.red);
        }
    }
    private void RegisterButtonClick()
    {
        if (NetManager.Instance.IsConnectedClient || NetManager.Instance.InitClient())
        {
            UISystem.Show<UI_RegisterWindow>();
        }
        else
        {
            UISystem.Show<UI_MessagePopupWindow>().ShowMessage("Net error", Color.red);
        }
    }
    private void QuitButtonClick()
    {
        Application.Quit();
    }
}
