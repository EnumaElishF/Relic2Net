//使用PlayerManager和PlayerController，作为公共的部分，既会出现在Client又会出现在Server，
//从而要求他们不能去依赖客户端或者服务端的程序集的内容，要打断他们的依赖关系，可以通过事件，去传，从而跨程序集通信
using JKFrame;
using Cinemachine;
using UnityEngine;
/// <summary>
/// PlayerManager放到了热更新程序集HotUpdate里，那么就作为 只有客户端使用
/// </summary>
public class PlayerManager : SingletonMono<PlayerManager>
{
    [SerializeField] private CinemachineFreeLook cinemachine;
    public static PlayerController localPlayer { get; private set; }
    /// <summary>
    /// PlayerManaget在Awake里触发，然后PlayerController才在后续方法里面触发
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        EventSystem.AddTypeEventListener<InitLocalPlayerEvent>(OnInitLocalPlayerEvent);
    }

    private void OnInitLocalPlayerEvent(InitLocalPlayerEvent arg)
    {
        InitLocalPlayer(arg.localPlayer);
    }

    public void InitLocalPlayer(PlayerController player)
    {
        localPlayer = player;
        cinemachine.transform.position = localPlayer.Transform.position;
        cinemachine.LookAt = localPlayer.cameraLookatTarget;
        cinemachine.Follow = localPlayer.cameraFollowTarget;
    }
}

