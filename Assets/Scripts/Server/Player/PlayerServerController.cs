using JKFrame;
using System;
using UnityEngine;
//给玩家使用
public class PlayerServerController : CharacterServerControllerBase<PlayerController>, IPlayerServerController,IStateMachineOwner
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
    #endregion

    public InputData inputData { get; private set; }
    public WeaponConfig weaponConfig { get; private set; }
    public bool Living => gameObject.activeInHierarchy && mainController.currentHp.Value > 0;

    public override void FirstInit(PlayerController mainController)
    {
        //放玩家特有的
        base.FirstInit(mainController);
        characterController = GetComponent<CharacterController>();
        inputData = new InputData();
        rootMotionMoveSpeedMultiply = ServerGlobal.Instance.ServerConfig.rootMotionMoveSpeedMultiply;
        airMoveSpeed = ServerGlobal.Instance.ServerConfig.playerAirMoveSpeed;
        gravity = ServerGlobal.Instance.ServerConfig.playerGravity;
        rotateSpeed = ServerGlobal.Instance.ServerConfig.playerRotateSpeed;
        jumpHeightMultiply = ServerGlobal.Instance.ServerConfig.playerJumpHeightMultiply;
        mainController.onUpdateWeaponObjectAction += MainController_onUpdateWeaponObjectAction;
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


    #region 网络
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ChangeState(PlayerState.Idle);
    }

    protected override void OnInitAOI()
    {
        AOIManager.Instance.InitClient(mainController.OwnerClientId, currentAOICoord);
    }

    protected override void OnUpdateAOI(Vector2Int newCoord)
    {
        AOIManager.Instance.UpdateClientChunkCoord(mainController.OwnerClientId, currentAOICoord, newCoord);
    }
    protected override void OnRemoveAOI()
    {
        AOIManager.Instance.RemoveClient(mainController.OwnerClientId, currentAOICoord);
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

    #region 战斗
    private void OnHit(IHitTarget target, Vector3 vector)
    {
        ((PlayerAttackState)stateMachine.currStateObj).OnHit(target, vector);
    }
    #endregion
}
