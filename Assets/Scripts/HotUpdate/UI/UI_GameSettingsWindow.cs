using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameSettingsWindow : UI_CustomWindowBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button languagePrevButton;
    [SerializeField] private Button languageNextButton;
    [SerializeField] private Image languageImage;
    [SerializeField] private Sprite englishIcon;
    [SerializeField] private Sprite chineseIcon;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectVolumeSlider;
    public override void Init()
    {
        closeButton.onClick.AddListener(CloseButtonClick);
        languagePrevButton.onClick.AddListener(LanguageButtonClick);
        languageNextButton.onClick.AddListener(LanguageButtonClick);
        musicVolumeSlider.onValueChanged.AddListener(MusicVolumeSliderValueChanged);
        soundEffectVolumeSlider.onValueChanged.AddListener(SoundEffectVolumeSliderValueChanged);
    }

    public override void OnShow()
    {
        base.OnShow();
        SetLanguageIcon(ClientGlobal.Instance.basicSetting.languageType);
        musicVolumeSlider.SetValueWithoutNotify(ClientGlobal.Instance.gameSetting.musicVolume);
        soundEffectVolumeSlider.SetValueWithoutNotify(ClientGlobal.Instance.gameSetting.soundEffectVolume);
    }
    private void CloseButtonClick()
    {
        UISystem.Close<UI_GameSettingsWindow>();
    }

    private void LanguageButtonClick()
    {
        LocalizationSystem.LanguageType = LocalizationSystem.LanguageType == LanguageType.SimplifiedChinese ? LanguageType.English : LanguageType.SimplifiedChinese;
        ClientGlobal.Instance.basicSetting.languageType = LocalizationSystem.LanguageType;
        SetLanguageIcon(LocalizationSystem.LanguageType);
    }

    private void SetLanguageIcon(LanguageType languageType)
    {
        languageImage.sprite = languageType == LanguageType.SimplifiedChinese ? chineseIcon : englishIcon;
    }
    /// <summary>
    /// 音量修改
    /// </summary>
    private void MusicVolumeSliderValueChanged(float newValue)
    {
        ClientGlobal.Instance.gameSetting.musicVolume = newValue;
        AudioSystem.BGVolume = newValue;
        EventSystem.TypeEventTrigger<MusicVolumeChangedEvent>(default);
    }
    /// <summary>
    /// 特效音乐
    /// </summary>
    private void SoundEffectVolumeSliderValueChanged(float newValue)
    {
        ClientGlobal.Instance.gameSetting.soundEffectVolume = newValue;
        AudioSystem.EffectVolume = newValue;
    }
    public override void OnClose()
    {
        base.OnClose();
        //关闭时保存设置到磁盘
        ClientGlobal.Instance.SaveGameSetting();
        ClientGlobal.Instance.SaveGameBasicSetting();
    }
}
