using JKFrame;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 公共   : 公共的地方大部分都是做分支的，分成客户端，和 服务端
/// </summary>
public partial class PlayerController : NetworkBehaviour
{
    private NetworkVariable<PlayerState> currentState = new NetworkVariable<PlayerState>(PlayerState.None);
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

    //相当于调用 "服务端" 上自身的本体
    //---也就是说，完成上面的数据判断后，把最终的坐标移动交给了服务器。
    //这就是Rpc好用的地方。
    //这里的moveSpeed就算本地客户端作弊，修改了数据，但是服务端在使用的时候用的是服务端的moveSpeed，还达到了防作弊效果。
    [ServerRpc(RequireOwnership =false)]  //只会被服务端调用的方法
    private void SendInputMoveDirServerRpc(Vector2 inputDir)
    {

#if UNITY_SERVER || UNITY_EDITOR
        Server_ReceiveMoveInput(inputDir);
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
    private Vector2 lastInputDir = Vector2.zero;
    //玩家移动输入判断 （客户端）
    private void LocalClientUpdate()
    {
        if (currentState.Value == PlayerState.None) return;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(h,v);
        if(inputDir == lastInputDir)
        {
            return;
        }
        lastInputDir = inputDir;

        SendInputMoveDirServerRpc(inputDir);

    }
}
#endif


/// <summary>
/// 服务端
/// </summary>
#if UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController : NetworkBehaviour
{
    #region 内部类型
    public class InputData
    {
        public Vector2 moveDir;
    }
    #endregion

    #region  面板赋值
    [SerializeField] private float moveSpeed = 3;
    public float MoveSpeed  { get => moveSpeed; }

    [SerializeField] private CharacterController characterController;
    public  CharacterController CharacterController { get => characterController; }

    [SerializeField] private Player_View view;
    public Player_View View { get => view; }

    [SerializeField] private Animator animator;
    public Animator Animator { get => animator; }
    #endregion


    public Vector2Int currentAOICoord { get; private set; }
    public InputData inputData { get; private set; }
    //框架，玩家使用的状态机
    private StateMachine stateMachine;


    private void Server_OnNetworkSpawn()
    {
        stateMachine = new StateMachine();
        inputData = new InputData();
        //登录游戏后，所在的位置，对应当前的AOI的坐标
        currentAOICoord = AOIUtility.GetCoordByWorldPostion(transform.position);
        AOIUtility.AddPlayer(this, currentAOICoord);
        ChangeState(PlayerState.Idle);
    }
    /// <summary>
    /// 服务端 把输入保存起来
    /// </summary>
    /// <param name="inputDir"></param>
    private void Server_ReceiveMoveInput(Vector2 inputDir)
    {
        inputData.moveDir = inputDir.normalized; //序列化，可以避免客户端去作弊
        //状态类中根据输入情况进行运算

    }

    /// <summary>
    /// 服务端玩家下线
    /// </summary>
    private void Server_OnNetworkDespawn()
    {
        AOIUtility.RemovePlayer(this, currentAOICoord);
    }

    public void ChangeState(PlayerState newState)
    {
        currentState.Value = newState;
        switch (newState)
        {
            case PlayerState.Idle:
                stateMachine.ChangeState<PlayerIdleState>();
                break;
            case PlayerState.Move:
                stateMachine.ChangeState<PlayerMoveState>();
                break;

        }
    }
    /// <summary>
    /// 采用自己管理的方式，状态切换的代码，控制动作状态改变。不使用常规的状态机的SetBool动作切换
    /// </summary>
    /// <param name="animationName"></param>
    public void PlayAnimation(string animationName, float fixedTransitionDuration = 0.25f)
    {
        animator.CrossFadeInFixedTime(animationName, fixedTransitionDuration); //默认动作过渡时间0.25秒，基本不用动
    }

    public void UpdateAOICoord()
    {
        //玩家开始移动
        Vector2Int newCoord = AOIUtility.GetCoordByWorldPostion(transform.position);
        Vector2Int oldCoord = currentAOICoord;
        if (newCoord != oldCoord) //发生了地图块的坐标变化
        {
            AOIUtility.UpdatePlayerCoord(this, oldCoord, newCoord);
            currentAOICoord = newCoord;
        }
    }
}
#endif
