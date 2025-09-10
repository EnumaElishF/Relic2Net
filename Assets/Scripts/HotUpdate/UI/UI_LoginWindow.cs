using JKFrame;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class UI_LoginWindow : UI_WindowBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Toggle rememberAccountToggle;

    public override void Init()
    {
        closeButton.onClick.AddListener(CloseButtonClick);
        submitButton.onClick.AddListener(SubmitButtonClick);
        registerButton.onClick.AddListener(RegisterButtonClick);
        nameInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);
        passwordInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);

    }
    public override void OnShow()
    {
        base.OnShow();
        submitButton.interactable = false;//登录按钮不可交互
        GameSetting gameSetting = ClientGlobal.Instance.gameSetting;
        nameInputField.text = gameSetting.rememberPlayerName != null ? gameSetting.rememberPlayerName : "";
        //重新展示的时候密码 如果有存档，就使用存档数据，否则使用""置空
        passwordInputField.text = gameSetting.rememberPassword != null ? gameSetting.rememberPassword : "";

        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_Login, OnS_C_Login);
    }

    public override void OnClose()
    {
        base.OnClose();
        //取消注册，如果有注册那就一定要有关闭
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S_C_Login, OnS_C_Login);
    }



    private void CloseButtonClick()
    {
        UISystem.Close<UI_LoginWindow>();
    }


    private void RegisterButtonClick()
    {
        UISystem.Close<UI_LoginWindow>();
        UISystem.Show<UI_RegisterWindow>();

    }
    private void OnInputFieldsValueChanged(string arg0)
    {
        submitButton.interactable = AccountFormatUtility.CheckName(nameInputField.text)
            && AccountFormatUtility.CheckPassword(passwordInputField.text);
    }
    private void SubmitButtonClick()
    {
        submitButton.interactable = false;
        //记住账号
        if (rememberAccountToggle.isOn)
        {
            ClientGlobal.Instance.RememberAccount(nameInputField.text, passwordInputField.text);
        }

        //TODO 
        //客户端发给服务端: 登录请求
        NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_Login, new C_S_Login
        {
            accountInfo = new AccountInfo
            {
                playerName = nameInputField.text,
                password = passwordInputField.text
            }
        });
    }
    private void OnS_C_Login(ulong clientID, INetworkSerializable serializable)
    {
        submitButton.interactable = true;
        //接收
        S_C_Login netMessage = (S_C_Login)serializable;
        Debug.Log("errorCode是:" + netMessage.errorCode);
        if (netMessage.errorCode == ErrorCode.None)
        {
            //TODO 登录成功后续有很多要加的，暂时用弹窗提醒显示注册成功
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalizationKey("注册已成功", Color.green);


        }
        else
        {
            //注册Error
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalizationKey(netMessage.errorCode.ToString(), Color.red);
        }
    }
}
