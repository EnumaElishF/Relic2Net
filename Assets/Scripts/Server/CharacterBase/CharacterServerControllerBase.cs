using JKFrame;
using System.Collections;
using Unity.Netcode.Components;
using UnityEngine;
//通用给：Player 和 敌人
public abstract class CharacterServerControllerBase<M> : MonoBehaviour, ICharacterServerController,IStateMachineOwner where M: CharacterControllerBase
{
    #region 通用部分，不论是怪还是玩家
    public Animator animator { get; protected set; }
    public NetworkAnimator networkAnimator { get; protected set; }
    public M mainController { get; protected set; } //主控制器M，PlayerController+MonsterController
    public Vector2Int currentAOICoord { get; protected set; }
    public WeaponController weapon { get; protected set; }

    protected StateMachine stateMachine; //框架的类，玩家使用的状态机
    #endregion

    public virtual void FirstInit(M mainController)
    {
        //把玩家和敌人，共有的放这里
        this.mainController = mainController;
        animator = mainController.CharacterView.GetComponent<Animator>();
        networkAnimator = animator.GetComponent<NetworkAnimator>();
        stateMachine = new StateMachine();
        mainController.ServerController = this;
    }
    public virtual void Init()
    {
        currentAnimation = "Idle";
    }
    public virtual void OnNetworkSpawn()
    {
        stateMachine.Init(this);
        StartCoroutine(DoInitAOI());
    }
    public virtual void OnNetworkDespawn()
    {
        //玩家销毁时，StateMachine要销毁
        stateMachine.Destroy();
        OnRemoveAOI();
    }


    #region AOI的网络部分

    /// <summary>
    /// 角色生成延迟同步避免发送两次网络对象生成，
    /// 这是NetCode的一个Bug，生成后立刻NetworkShow产生的，延迟一帧即可。
    /// 这个问题解决起来简单，但是发现起来比较复杂
    /// </summary>
    private IEnumerator DoInitAOI()
    {
        //延迟一帧
        //使用框架的方法，这个其实工具好处就是内置了一些可以重复利用的对象，避免一些gc的问题
        yield return CoroutineTool.WaitForFrame();
        //登录游戏后，所在的位置，对应当前的AOI的坐标
        currentAOICoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        OnInitAOI();
    }
    protected abstract void OnInitAOI();
    protected abstract void OnRemoveAOI();

    public void UpdateAOICoord()
    {
        //玩家开始移动
        Vector2Int newCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        if (newCoord != currentAOICoord) // 发生了地图块坐标变化
        {
            OnUpdateAOI(newCoord);
            currentAOICoord = newCoord;
        }
    }
    protected abstract void OnUpdateAOI(Vector2Int newCoord);
    #endregion

    #region 动画
    public string currentAnimation { get; protected set; }

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

    /// <summary>
    /// 解决动作已经进入下一个动画，但是还处于上一个动画的最后结束帧的事件
    /// </summary>
    public bool CheckAnimationState(string stateName, out float normalizedTime)
    {
        //优先判断下一个动作状态
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
        if (nextInfo.IsName(stateName))
        {
            normalizedTime = nextInfo.normalizedTime;
            return true;
        }
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
        normalizedTime = currentInfo.normalizedTime;
        return currentInfo.IsName(stateName);
    }
    #endregion

}
