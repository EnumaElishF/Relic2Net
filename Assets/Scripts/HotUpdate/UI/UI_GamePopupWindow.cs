using JKFrame;
using UnityEngine;
using UnityEngine.UI;


public class UI_GamePopupWindow : UI_CustomWindowBase
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button quitButton;
    public override void Init()
    {
        continueButton.onClick.AddListener(ContinueButtonClick);
        settingButton.onClick.AddListener(SettingButtonClick);
        backButton.onClick.AddListener(BackButtonClick);
        quitButton.onClick.AddListener(QuitButtonClick);
    }

    private void ContinueButtonClick()
    {
        UISystem.Close<UI_GamePopupWindow>();
    }

    private void SettingButtonClick()
    {
        //游戏设置UI窗口
        UISystem.Show<UI_GameSettingsWindow>();
    }

    private void BackButtonClick()
    {
        // 退出到菜单场景
        NetManager.Instance.StopClient();
        ClientGlobal.Instance.EnterLoginScene();
        //NetMessageManager.Instance.SendMessageToServer<C_S_Disconnect>(MessageType.C_S_Disconnect, default);
        //等待服务端回复消息后退出到登录场景，改为由ClientGlobal来监听。
        //ClientGlobal.Instance.EnterLoginScene();
    }

    private void QuitButtonClick()
    {
        // 完全关闭应用不用理会网络连接问题，因为Netcode会自动处理
        Application.Quit();
    }
}

