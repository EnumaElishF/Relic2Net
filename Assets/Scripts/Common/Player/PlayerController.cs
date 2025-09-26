using JKFrame;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 公共   : 公共的地方大部分都是做分支的，分成客户端，和 服务端
/// </summary>
public partial class PlayerController : NetworkBehaviour
{
    #region 静态成员 static
    private static Func<string, GameObject> getWeaponFunc;
    public static void SetGetWeaponFunc(Func<string,GameObject> func)
    {
        getWeaponFunc = func;
        Debug.Log($"委托绑定：{func.Method.DeclaringType.Name}.{func.Method.Name}"); // 输出绑定的方法所在类
    }
    #endregion

    [SerializeField] private Player_View view;
    public Player_View View { get => view; }
    private NetVariable<PlayerState> currentState = new NetVariable<PlayerState>(PlayerState.None);
    //新程序集，要加入对Common的新程序集的引用 Unity.Collections;
    public NetVariable<FixedString32Bytes> usedWeaponName = new NetVariable<FixedString32Bytes>();

    //多个玩家，所以Player没有单例
    //public NetworkVariable<float> moveSpeed;   //网络变量：值类型，或者是结构体

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        usedWeaponName.OnValueChanged = OnUsedWeaponNameChanged;

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
        UpdateWeaponObject(usedWeaponName.Value.ToString());
    }

    private void OnUsedWeaponNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        UpdateWeaponObject(newValue.ToString());
    }
    private void UpdateWeaponObject(string weaponID)
    {
        if (string.IsNullOrWhiteSpace(weaponID)) return; //weapon上关于playerData的数据还没执行获得到呢的话，就不先执行下面的内容
        GameObject weaponGameObject = getWeaponFunc.Invoke(weaponID);
        view.SetWeapon(weaponGameObject);
    }

    /// <summary>
    /// 玩家下线，Despawn消除玩家
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            
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
    //???是true还是false
    [ServerRpc(RequireOwnership = false)]  //只会被服务端调用的方法
    private void SendInputMoveDirServerRpc(Vector3 moveDir)
    {

#if UNITY_SERVER || UNITY_EDITOR
        Server_ReceiveMoveInput(moveDir);
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
    public bool canControl; //玩家是否可以控制
    private void Start()
    {
        // Start 一定 在OnNetworkSpawn后执行，如果这个阶段IsSpawned = false 说明是个异常对象
        if (!IsSpawned)
        {
            //网络对象对象池
            //玩家如果退出登录,用这个可以销毁，但是我们有AOI相关的东西，不能仅仅把游戏对象销毁就结束。 
            NetManager.Instance.DestroyObject(this.NetworkObject);
        }
    }


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
    //最后输入
    private Vector3 lastInputDir = Vector3.zero;
    //玩家移动输入判断 （客户端）
    private void LocalClientUpdate()
    {
        if (currentState.Value == PlayerState.None) return;

        //因为玩家发像服务端的移动是如果一个键没有变化就一直向服务端发，
        //---(我们的移动指令设计是这样的，所以鼠标暂停移动，不能直接在这里return断掉键位消息的发送情况，需要下面逻辑判断为不移动)
        Vector3 inputDir = Vector3.zero;
        if (canControl)
        {
            //如果true可以控制，就使用玩家键入的按键。如果false不能控制，那么就等于没按
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            inputDir = new Vector3(h, 0, v);
        }

        if(inputDir == Vector3.zero && lastInputDir == Vector3.zero) return;//上一次没按，这一次也没按，就直接不用传消息了

        //输入方向
        lastInputDir = inputDir;
        //计算摄像机的旋转，和相对的角色WASD移动的 对应变化
        float cameraEulerAngleY = Camera.main.transform.eulerAngles.y;
        //四元数和向量相乘：让这个向量按照四元数所表达的角度进行旋转后得到一个新的向量
        //移动方向：Quaternion.Euler(0, cameraEulerAngleY, 0) * inputDir
        SendInputMoveDirServerRpc(Quaternion.Euler(0, cameraEulerAngleY, 0) * inputDir);

    }
}
#endif


/// <summary>
/// 服务端
/// </summary>
#if UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController : NetworkBehaviour,IStateMachineOwner
{
    #region 内部类型
    public class InputData
    {
        public Vector3 moveDir;
    }
    #endregion

    #region  面板赋值 (理论上，下面这些值，包括移动速度旋转速度等，客户端都不需要知道，只要服务端知道就行) 服务端基于根运动移动
    [SerializeField] private float moveSpeed = 1;
    public float MoveSpeed  { get => moveSpeed; }

    [SerializeField] private float rotateSpeed = 1000; //至少一秒要能转1000度
    public float RotateSpeed { get => rotateSpeed; }

    [SerializeField] private CharacterController characterController;
    public  CharacterController CharacterController { get => characterController; }

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
        stateMachine.Init(this);
        inputData = new InputData();
        //登录游戏后，所在的位置，对应当前的AOI的坐标
        currentAOICoord = AOIUtility.GetCoordByWorldPostion(transform.position);
        Debug.Log("Server产生玩家");
        AOIUtility.AddPlayer(this, currentAOICoord);
        ChangeState(PlayerState.Idle);
    }
    /// <summary>
    /// 服务端 把输入保存起来
    /// </summary>
    private void Server_ReceiveMoveInput(Vector3 moveDir)
    {
        inputData.moveDir = moveDir.normalized; //序列化，可以避免客户端去作弊
        //状态类中根据输入情况进行运算

    }

    /// <summary>
    /// 服务端玩家下线
    /// </summary>
    private void Server_OnNetworkDespawn()
    {
        //玩家销毁时，StateMachine要销毁
        stateMachine.Destroy();
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
        if (newCoord != currentAOICoord) // 发生了地图块坐标变化
        {
            AOIUtility.UpdatePlayerCoord(this, currentAOICoord, newCoord);
            currentAOICoord = newCoord;
        }
    }
    /// <summary>
    /// 更新网络变量
    /// </summary>
    /// <param name="weaponID"></param>
    public void UpdateWeaponNetVar(string weaponID)
    {
        usedWeaponName.Value = weaponID;
    }
}
#endif
