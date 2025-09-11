
using System;

[Serializable] //要保存，所以希望他可以序列化
public class GameSetting
{
    public string rememberPlayerName;
    public string rememberPassword;
    public float musicVolume = 1f;
    public float soundEffectVolume = 1f;

}