using JKFrame;
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
        submitButton.interactable = false;//登录按钮不可交互
        GameSetting gameSetting = ClientGlobal.Instance.gameSetting;
        nameInputField.text = gameSetting.rememberPlayerName != null ? gameSetting.rememberPlayerName : "";
        //重新展示的时候密码 如果有存档，就使用存档数据，否则使用""置空
        passwordInputField.text = gameSetting.rememberPassword != null ? gameSetting.rememberPassword : "";


        //注册
        //NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_Register, OnS_C_Register);
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
        //记住账号
        if (rememberAccountToggle.isOn)
        {
            ClientGlobal.Instance.RememberAccount(nameInputField.text, passwordInputField.text);
        }

        //TODO 
        submitButton.interactable = false;
        //客户端发给服务端
        //NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_Register, new C_S_Register
        //{
        //    accountInfo = new AccountInfo
        //    {
        //        playerName = nameInputField.text,
        //        password = passwordInputField.text
        //    }
        //});
    }
}
