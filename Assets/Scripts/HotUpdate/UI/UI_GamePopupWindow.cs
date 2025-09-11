using JKFrame;
using UnityEngine;
using UnityEngine.UI;


public class UI_GamePopupWindow : UI_WindowBase
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
        //UISystem.Show<UI_GameSettingsWindow>();
    }

    private void BackButtonClick()
    {
        // 退出到菜单场景
        //NetManager.Instance.StopClient();
        //ClientGlobal.Instance.EnterLoginScene();
    }

    private void QuitButtonClick()
    {
        // 完全关闭应用不用理会网络连接问题，因为Netcode会自动处理
        Application.Quit();
    }
}

