using JKFrame;
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

        submitButton.interactable = false;//登录按钮不可交互
    }
    public override void OnShow()
    {

        //重新展示的时候密码置空
        passwordInputField.text = "";
        rePasswordInputField.text = "";
    }
    private void CloseButtonClick()
    {
        UISystem.Close<UI_RegisterWindow>();
    }


    private void LoginButtonClick()
    {
        //TODO
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
    }
}
