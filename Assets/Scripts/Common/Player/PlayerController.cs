using JKFrame;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 公共   : 公共的地方大部分都是做分支的，分成客户端，和 服务端
/// </summary>
public partial class PlayerController : NetworkBehaviour
{
    //多个玩家，所以Player没有单例
    //public NetworkVariable<float> moveSpeed;   //网络变量：值类型，或者是结构体

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            Client_OnNetworkSpawn();
#endif
        }
        else
        {
#if UNITY_SERVER || UNITY_EDITOR
            Server_OnNetworkSpawn();
#endif
        }

    }

    /// <summary>
    /// 玩家下线，Despawn消除玩家
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            //Client_OnNetworkSpawn();
#endif
        }
        else
        {
#if UNITY_SERVER || UNITY_EDITOR
            Server_OnNetworkDespawn();
#endif
        }
    }

    //相当于调用服务端上自身的本体
    //---也就是说，完成上面的数据判断后，把最终的坐标移动交给了服务器。
    //这就是Rpc好用的地方。
    //这里的moveSpeed就算本地客户端作弊，修改了数据，但是服务端在使用的时候用的是服务端的moveSpeed，还达到了防作弊效果。
    [ServerRpc(RequireOwnership =false)]  //只会被服务端调用的方法
    private void HandleMovementServerRpc(Vector3 inputDir)
    {

#if UNITY_SERVER || UNITY_EDITOR
        Movement(inputDir);
#endif

    }


}

/// <summary>
/// 客户端
/// </summary>
#if !UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController : NetworkBehaviour
{
    public Transform cameraLookatTarget;
    public Transform cameraFollowTarget;
    private void Client_OnNetworkSpawn()
    {
        //IsOwner 是一个布尔值，用于判断当前本地客户端是否是该 Network Object 的 “所有者”（Owner）。
        if (IsOwner)
        {
            //客户端快速访问到 “自己控制的玩家对象”
            //PlayerManager.Instance.InitLocalPlayer(this);  替换为下方传事件
            EventSystem.TypeEventTrigger(new InitLocalPlayerEvent { localPlayer = this });

            this.AddUpdate(LocalClientUpdate);//添加一个Update的监听,框架做的
        }
    }

    private void LocalClientUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;
        HandleMovementServerRpc(inputDir);

    }
}
#endif
/// <summary>
/// 服务端
/// </summary>
#if UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController : NetworkBehaviour
{
   [SerializeField] public float moveSpeed = 3;
    public float MoveSpeed  { get => moveSpeed; }
    private Vector2Int currentAOICoord;
    private void Server_OnNetworkSpawn()
    {
        //登录游戏后，所在的位置，对应当前的AOI的坐标
        currentAOICoord = AOIUtility.GetCoordByWorldPostion(transform.position);
        AOIUtility.AddPlayer(this, currentAOICoord);
    }
    private void Movement(Vector2 inputDir)
    {
        inputDir.Normalize();
        //告诉服务端，有这个事情
        transform.Translate(Time.deltaTime * moveSpeed * inputDir);

        Vector2Int newCoord = AOIUtility.GetCoordByWorldPostion(transform.position);
        Vector2Int oldCoord = currentAOICoord;
        if (newCoord != oldCoord) //发生了地图块的坐标变化
        {
            AOIUtility.UpdatePlayerCoord(this, oldCoord, newCoord);
            currentAOICoord = newCoord;
        }
    }

    /// <summary>
    /// 服务端玩家下线
    /// </summary>
    private void Server_OnNetworkDespawn()
    {
        AOIUtility.RemovePlayer(this, currentAOICoord);
    }
}
#endif
