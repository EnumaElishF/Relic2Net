
/// <summary>
/// 账号格式校验
/// </summary>
public static class AccountFormatUtility
{
    public static bool CheckName(string name)
    {
        return !string.IsNullOrEmpty(name) && name.Length >= 5 && name.Length <= 12;
    }
    public static bool CheckPassword(string pawssword)
    {
        return !string.IsNullOrEmpty(pawssword) && pawssword.Length >= 6 && pawssword.Length <= 16;
    }
}