using JKFrame;
using System;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerServerController : MonoBehaviour, IPlayerServerController,IStateMachineOwner
{
    public class InputData
    {
        public Vector3 moveDir;
        public bool jump;
        public bool attack;
    }
    #region  面板赋值 (理论上，下面这些值，包括移动速度旋转速度等，客户端都不需要知道，只要服务端知道就行) 服务端基于根运动移动
    public float rootMotionMoveSpeedMultiply { get; private set; } //动画根运动的系数(是系数的情况加个Multiply
    public float airMoveSpeed { get; private set; }
    public float gravity { get; private set; }
    public float rotateSpeed { get; private set; }
    public float jumpHeightMultiply { get; private set; }
    public CharacterController characterController { get; private set; }
    public Animator animator { get; private set; }
    public NetworkAnimator networkAnimator { get; private set; }
    #endregion

    public PlayerController mainController { get; private set; }
    public Vector2Int currentAOICoord { get; private set; }
    public InputData inputData { get; private set; }
    public WeaponController weapon { get; private set; }
    public WeaponConfig weaponConfig { get; private set; }
    //框架，玩家使用的状态机
    private StateMachine stateMachine;
    public void FirstInit(PlayerController mainController)
    {
        this.mainController = mainController;
        characterController = GetComponent<CharacterController>();
        animator = transform.Find("Player_kazuma").GetComponent<Animator>();
        networkAnimator = animator.GetComponent<NetworkAnimator>();
        stateMachine = new StateMachine();
        inputData = new InputData();

        rootMotionMoveSpeedMultiply = ServerGlobal.Instance.ServerConfig.rootMotionMoveSpeedMultiply;
        airMoveSpeed = ServerGlobal.Instance.ServerConfig.playerAirMoveSpeed;
        gravity = ServerGlobal.Instance.ServerConfig.playerGravity;
        rotateSpeed = ServerGlobal.Instance.ServerConfig.playerRotateSpeed;
        jumpHeightMultiply = ServerGlobal.Instance.ServerConfig.playerJumpHeightMultiply;

        if(mainController == null)
        {
            Debug.Log(mainController + "mainController为空");
        }
        mainController.SetServerController(this);
        mainController.onUpdateWeaponObjectAction += MainController_onUpdateWeaponObjectAction;
    }
    public void Init()
    {
        currentAnimation = "Idle";
    }
    private void MainController_onUpdateWeaponObjectAction(GameObject obj)
    {
        if(!obj.TryGetComponent(out WeaponController temp))
        {
            temp = obj.AddComponent<WeaponController>();
        }
        weapon = temp;
        weapon.Init("PlayerWeapon", OnHit);
        weaponConfig = ServerResSystem.GetItemConfig<WeaponConfig>(mainController.usedWeaponName.Value.ToString());
    }

    #region 网络
    public void OnNetworkSpawn()
    {
        stateMachine.Init(this);
        //登录游戏后，所在的位置，对应当前的AOI的坐标
        currentAOICoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        Debug.Log("Server产生玩家");
        //AOI添加玩家
        AOIManager.Instance.InitClient(mainController.OwnerClientId, currentAOICoord);
        ChangeState(PlayerState.Idle);
    }
    public void OnNetworkDespawn()
    {
        //玩家销毁时，StateMachine要销毁
        stateMachine.Destroy();
        AOIManager.Instance.RemoveClient(mainController.OwnerClientId, currentAOICoord);
    }
    public void UpdateAOICoord()
    {
        //玩家开始移动
        Vector2Int newCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        if (newCoord != currentAOICoord) // 发生了地图块坐标变化
        {
            AOIManager.Instance.UpdateClientChunkCoord(mainController.OwnerClientId, currentAOICoord, newCoord);
            currentAOICoord = newCoord;
        }
    }
    #endregion

    #region 响应客户端的输入
    public void ReceiveMoveInput(Vector3 moveDir) 
    {
        inputData.moveDir = moveDir.normalized; //序列化，可以避免客户端去作弊
        //状态类中根据输入情况进行运算
    }
    public void ReceiveJumpInput()
    {
        switch (mainController.currentState.Value)
        {
            case PlayerState.Idle:
            case PlayerState.Move:
                inputData.jump = true;
                break;
        }
    }
    public void ReceiveAttackInput(bool value)
    {
        switch (mainController.currentState.Value)
        {
            case PlayerState.Idle:
            case PlayerState.Move:
            case PlayerState.Attack:
                inputData.attack = value;
                break;
        }
    }
    #endregion
    //其他条件

    public void ChangeState(PlayerState newState)
    {
        mainController.currentState.Value = newState;
        switch (newState)
        {
            case PlayerState.Idle:
                stateMachine.ChangeState<PlayerIdleState>();
                break;
            case PlayerState.Move:
                stateMachine.ChangeState<PlayerMoveState>();
                break;
            case PlayerState.Jump:
                stateMachine.ChangeState<PlayerJumpState>();
                break;
            case PlayerState.AirDown:
                stateMachine.ChangeState<PlayerAirDownState>();
                break;
            case PlayerState.Attack:
                stateMachine.ChangeState<PlayerAttackState>();
                break;
        }
    }
    private string currentAnimation = "Idle";
    /// <summary>
    /// 采用自己管理的方式，状态切换的代码，控制动作状态改变。不使用常规的状态机的SetBool动作切换
    /// </summary>
    public void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName) return;
        networkAnimator.ResetTrigger(currentAnimation);
        currentAnimation = animationName;
        networkAnimator.SetTrigger(animationName);
    }

    #region 战斗
    private void OnHit(IHitTarget target, Vector3 vector)
    {
        ((PlayerAttackState)stateMachine.currStateObj).OnHit(target, vector);
    }
    #endregion
}
