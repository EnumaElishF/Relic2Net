using JKFrame;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_RegisterWindow : UI_WindowBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private InputField nameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private InputField rePasswordInputField;
    public override void Init()
    {
        closeButton.onClick.AddListener(CloseButtonClick);
        submitButton.onClick.AddListener(SubmitButtonClick);
        loginButton.onClick.AddListener(LoginButtonClick);

        nameInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);
        passwordInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);
        rePasswordInputField.onValueChanged.AddListener(OnInputFieldsValueChanged);

    }
    public override void OnShow()
    {
        submitButton.interactable = false;//登录按钮不可交互

        //重新展示的时候密码置空
        passwordInputField.text = "";
        rePasswordInputField.text = "";

        //注册
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_Register, OnS_C_Register);
    }
    public override void OnClose()
    {
        //取消注册，如果有注册那就一定要有关闭
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S_C_Register, OnS_C_Register);

    }

    private void CloseButtonClick()
    {
        UISystem.Close<UI_RegisterWindow>();
    }


    private void LoginButtonClick()
    {
        UISystem.Close<UI_RegisterWindow>();
        UISystem.Show<UI_LoginWindow>();
    }

    private void OnInputFieldsValueChanged(string arg0)
    {
        submitButton.interactable = AccountFormatUtility.CheckName(nameInputField.text)
            && AccountFormatUtility.CheckPassword(passwordInputField.text)
            && passwordInputField.text == rePasswordInputField.text;
    }
    private void SubmitButtonClick()
    {
        //TODO 
        submitButton.interactable = false;
        //客户端发给服务端
        NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_Register, new C_S_Register
        {
            accountInfo = new AccountInfo
            {
                playerName = nameInputField.text,
                password = passwordInputField.text
            }
        });
    }
    private void OnS_C_Register(ulong clientID, INetworkSerializable serializable)
    {
        //接收
        S_C_Register netMessage = (S_C_Register)serializable;
        if (netMessage.errorCode == ErrorCode.None)
        {
            //直接用了color，正确用green，警告用yellow，错误用red
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalizationKey("注册已成功", Color.green);
        }
        else
        {
            //注册Error
            UISystem.Show<UI_MessagePopupWindow>().ShowMessageByLocalizationKey(netMessage.errorCode.ToString(), Color.red);
        }
        submitButton.interactable = true;

    }
}
