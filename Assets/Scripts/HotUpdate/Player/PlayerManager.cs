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
    /// PlayerManaget在Awake里触发xxx改为手动Init，然后要求PlayerController才在后续方法里面触发
    /// </summary>
    public void Init()
    {
        ClientGlobal.Instance.ActiveMouse = false;
        EventSystem.AddTypeEventListener<InitLocalPlayerEvent>(OnInitLocalPlayerEvent);
    }
    private void OnDestroy()
    {
        //检查所有的的EventSystem的事件绑定，因为PlayerManager脚本并不是全局的，他会因为场景卸载而关闭
        //那么就需要把事件取消掉，所以加了下面这个RemoveTypeEventListener移除事件监听
        //以此类似的还有很多，但是像是ClientGlobal这种一直存在，就不需要加下面的处理
        EventSystem.RemoveTypeEventListener<InitLocalPlayerEvent>(OnInitLocalPlayerEvent);
    }

    private void OnInitLocalPlayerEvent(InitLocalPlayerEvent arg)
    {
        InitLocalPlayer(arg.localPlayer);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI_GamePopupWindow gamePopupWindow = UISystem.GetWindow<UI_GamePopupWindow>();
            if (gamePopupWindow == null || !gamePopupWindow.gameObject.activeInHierarchy)
            {
                UISystem.Show<UI_GamePopupWindow>();
            }
            else
            {
                UISystem.Close<UI_GamePopupWindow>();
            }
        }
    }
    public bool IsLoadingCompleted()
    {
        //不为null了，则说明玩家加载完成了
        return localPlayer != null;
    }

    public void InitLocalPlayer(PlayerController player)
    {
        localPlayer = player;
        cinemachine.transform.position = localPlayer.transform.position;
        //注: Unity虽然会把这部分热更在打包前把他们剔除不在包体里，但是Unity剔除前依然会检验这部分能不能用
        //尤其是公共部分，一种打包成客户端，一种打包成服务端。需要考虑好这个东西在对立，比如服务端的热更新里会不会引用这个内容。反之，客户端也考虑一下。

        //例如：如果是服务端版的热更新的打包，下面这个是纯客户端才存在的东西，如果不加限制为 客户端的#if,那就会报错
#if !UNITY_SERVER || UNITY_EDITOR
        cinemachine.LookAt = localPlayer.cameraLookatTarget;
        cinemachine.Follow = localPlayer.cameraFollowTarget;
#endif
    }
}

