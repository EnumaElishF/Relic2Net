using JKFrame;


/// <summary>
///  
/// </summary>
public class ClientGlobal : SingletonMono<ClientGlobal>
{
    protected override void Awake()
    {
        base.Awake();
        //Global�и��ص㣬���ǲ�������
        DontDestroyOnLoad(gameObject);

        ResSystem.InstantiateGameObject<NetManager>("NetworkManager").Init(true);//ֱ���õ�ͬ������Ϊ�ļ���С��������˻���Ҫ���첽����

    }
}
