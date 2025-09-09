
/// <summary>
/// 账号格式校验
/// </summary>
public static class AccountFormatUtility
{
    public static bool CheckName(string name)
    {
        return name.Length >= 5 && name.Length <= 12;
    }
    public static bool CheckPassword(string pawssword)
    {
        return pawssword.Length >= 6 && pawssword.Length <= 16;
    }
}