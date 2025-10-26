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
    #region  面板赋值 (理论上，下面这些值，包括移动速度旋转速度等，客户端都不需要知道，只要服务端知道就行) 服务端基于根运动移动
    [SerializeField] private float moveSpeed = 1;
    public float MoveSpeed { get => moveSpeed; }
    [SerializeField] private float rotateSpeed = 1000; //至少一秒要能转1000度
    public float RotateSpeed { get => rotateSpeed; }
    [SerializeField] private CharacterController characterController;
    public CharacterController CharacterController { get => characterController; }
    [SerializeField] private Animator animator;
    public Animator Animator { get => animator; }
    public Transform cameraLookatTarget;
    public Transform cameraFollowTarget;
    public Transform floatInfoPoint;

    [SerializeField] private Player_View view;
    public Player_View View { get => view; }
    #endregion
    public NetVariable<PlayerState> currentState = new NetVariable<PlayerState>(PlayerState.None);
    //新程序集，要加入对Common的新程序集的引用 Unity.Collections;
    public NetVariable<FixedString32Bytes> usedWeaponName = new NetVariable<FixedString32Bytes>();
    public NetVariable<FixedString32Bytes> playerName = new NetVariable<FixedString32Bytes>();

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
    [ServerRpc(RequireOwnership = true)]  //只会被服务端调用的方法,RequireOwnership验证是否宿主
    public void SendInputMoveDirServerRpc(Vector3 moveDir)
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
        EventSystem.TypeEventTrigger(new SpawnPlayerEvent { newPlayer = this });
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
    /// 更新武器网络变量
    /// </summary>
    /// <param name="weaponID"></param>
    public void UpdateWeaponNetVar(string weaponID)
    {
        usedWeaponName.Value = weaponID;
    }
}
#endif
