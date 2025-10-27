using JKFrame;
using UnityEngine;

public class PlayerServerController : MonoBehaviour, IPlayerServerController,IStateMachineOwner
{
    public class InputData
    {
        public Vector3 moveDir;
    }
    #region  面板赋值 (理论上，下面这些值，包括移动速度旋转速度等，客户端都不需要知道，只要服务端知道就行) 服务端基于根运动移动
    public float moveSpeed { get; private set; }
    public float rotateSpeed { get; private set; }
    public float jumpHeightMultiply { get; private set; }
    public CharacterController characterController { get; private set; }
    public Animator animator { get; private set; }
    #endregion

    public PlayerController mainController { get; private set; }
    public Vector2Int currentAOICoord { get; private set; }
    public InputData inputData { get; private set; }
    //框架，玩家使用的状态机
    private StateMachine stateMachine;
    public void FirstInit()
    {
        characterController = GetComponent<CharacterController>();
        animator = transform.Find("Player_kazuma").GetComponent<Animator>();

        stateMachine = new StateMachine();
        inputData = new InputData();
    }
    public void Init(PlayerController mainController)
    {
        this.mainController = mainController;
        mainController.SetServerController(this);
        moveSpeed = ServerGlobal.Instance.ServerConfig.playerMoveSpeed;
        rotateSpeed = ServerGlobal.Instance.ServerConfig.playerRotateSpeed;
        jumpHeightMultiply = ServerGlobal.Instance.ServerConfig.playerJumpHeightMultiply;
    }
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

    public void ReceiveMoveInput(Vector3 moveDir) 
    {
        inputData.moveDir = moveDir.normalized; //序列化，可以避免客户端去作弊
        //状态类中根据输入情况进行运算
    }

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
        Vector2Int newCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        if (newCoord != currentAOICoord) // 发生了地图块坐标变化
        {
            AOIManager.Instance.UpdateClientChunkCoord(mainController.OwnerClientId, currentAOICoord, newCoord);
            currentAOICoord = newCoord;
        }
    }
}
