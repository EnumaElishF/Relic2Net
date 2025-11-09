using JKFrame;
using UnityEngine;
/// <summary>
/// ServerGlobal作为全局所做的，他是不会被销毁的
/// </summary>
public class ServerGlobal : SingletonMono<ServerGlobal>
{
    [SerializeField] private ServerConfig serverConfig;
    public ServerConfig ServerConfig { get => serverConfig; }
    public LayerMask PlayerLayerMask { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        PlayerLayerMask = LayerMask.GetMask("Player");
        DontDestroyOnLoad(gameObject);
        EventSystem.AddTypeEventListener<GameSceneLaunchEvent>(OnGameSceneLaunchEvent);
    }

    private void OnGameSceneLaunchEvent(GameSceneLaunchEvent @event)
    {
        ServerResSystem.InstantiateServerOnGameScene();
    }
}
