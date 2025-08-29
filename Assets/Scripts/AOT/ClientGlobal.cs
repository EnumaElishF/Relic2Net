using JKFrame;


/// <summary>
///  
/// </summary>
public class ClientGlobal : SingletonMono<ClientGlobal>
{
    protected override void Awake()
    {
        base.Awake();
        //Global有个特点，就是不能销毁
        DontDestroyOnLoad(gameObject);
    }
}
