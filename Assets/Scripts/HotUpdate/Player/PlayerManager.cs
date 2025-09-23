//使用PlayerManager和PlayerController，作为公共的部分，既会出现在Client又会出现在Server，
//从而要求他们不能去依赖客户端或者服务端的程序集的内容，要打断他们的依赖关系，可以通过事件，去传，从而跨程序集通信
using Cinemachine;
using JKFrame;
using System;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// PlayerManager放到了热更新程序集HotUpdate里，那么就作为 只有客户端使用
/// </summary>
public class PlayerManager : SingletonMono<PlayerManager>
{
    [SerializeField] private CinemachineFreeLook cinemachine;
    public static PlayerController localPlayer { get; private set; }
    //玩家是否可以控制角色，以后可能受到多个方面的影响，目前只和鼠标显示关联
    public bool playerControlEnable { get; private set; }
    private BagData bagData;
    /// <summary>
    /// PlayerManaget在Awake里触发xxx改为手动Init，然后要求PlayerController才在后续方法里面触发
    /// </summary>
    public void Init()
    {
        //事件的监听开始
        EventSystem.AddTypeEventListener<InitLocalPlayerEvent>(OnInitLocalPlayerEvent);
        EventSystem.AddTypeEventListener<MouseActiveStateChangedEvent>(OnMouseActiveStateChangedEvent);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_GetBagData, OnS_C_GetBagData);

        ClientGlobal.Instance.ActiveMouse = false;
    }



    private void OnDestroy()
    {
        //事件监听取消，针对内部方法
        //检查所有的的EventSystem的事件绑定，因为PlayerManager脚本并不是全局的，他会因为场景卸载而关闭
        //那么就需要把事件取消掉，所以加了下面这个RemoveTypeEventListener移除事件监听
        //以此类似的还有很多，但是像是ClientGlobal这种一直存在，就不需要加下面的处理
        EventSystem.RemoveTypeEventListener<InitLocalPlayerEvent>(OnInitLocalPlayerEvent);
        EventSystem.RemoveTypeEventListener<MouseActiveStateChangedEvent>(OnMouseActiveStateChangedEvent);//每次在ClientGlobal的ActiveMouse触发
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S_C_GetBagData, OnS_C_GetBagData);
    }

    private void OnInitLocalPlayerEvent(InitLocalPlayerEvent arg)
    {
        InitLocalPlayer(arg.localPlayer);
    }
    private void OnMouseActiveStateChangedEvent(MouseActiveStateChangedEvent arg)
    {
        //目前只和鼠标是否显示关联
        playerControlEnable = !arg.activeState;
        //cinemachine转向的相机控制
        cinemachine.enabled = playerControlEnable;
        if (localPlayer != null)
        {
            localPlayer.canControl = playerControlEnable;
        }
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

        //把背包消息发给服务器
        if (Input.GetKeyDown(KeyCode.B))
        {
            UI_BagWindow bagWindow = UISystem.GetWindow<UI_BagWindow>();
            if (bagWindow == null || !bagWindow.gameObject.activeInHierarchy) 
            {
                //请求网络
                int dataVersion = bagData == null ? -1 : bagData.dataVersion;
                NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_GetBagData, new C_S_GetBagData { dataVersion = dataVersion });
                //等网络消息回发

            }
            else
            {
                UISystem.Close<UI_BagWindow>();
            }
        }
    }
    /// <summary>
    /// 服务器把背包的消息发回来
    /// </summary>
    private void OnS_C_GetBagData(ulong serverID, INetworkSerializable serializable)
    {
        S_C_GetBagData message = (S_C_GetBagData)serializable;
        if(message.haveBagData && message.bagData != null)
        {
            this.bagData = message.bagData;
        }
        UISystem.Show<UI_BagWindow>().Show(bagData);
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
        localPlayer.canControl = playerControlEnable;
#endif
    }

    public void UseItem(int slotIndex)
    {
        //TODO 构建消息
    }
}

