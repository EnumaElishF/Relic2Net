
using JKFrame;

/// <summary>
/// Client的信息类
/// </summary>
public class Client 
{
    public ulong clientID; //NetCode的
    public ClientState clientState;
    /// <summary>
    /// 当销毁时，要做的事情
    /// </summary>
    public void OnDestroy()
    {
        this.ObjectPushPool();//放进对象池
    }
}
