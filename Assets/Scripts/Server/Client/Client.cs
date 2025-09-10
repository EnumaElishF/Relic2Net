
using JKFrame;

/// <summary>
/// Client的信息类_
/// </summary>
public class Client 
{
    public ulong clientID; //NetCode的
    public ClientState clientState;
    public PlayerData playerData;
    public PlayerController playerController;
    /// <summary>
    /// 当销毁时，要做的事情
    /// </summary>
    public void OnDestroy()
    {
        playerData = null;
        this.ObjectPushPool();//放进对象池
    }
}
