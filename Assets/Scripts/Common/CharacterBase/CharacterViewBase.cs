using Sirenix.OdinInspector;
using UnityEditor.Animations;
using System;
using UnityEngine;

public abstract class CharacterViewBase : SerializedMonoBehaviour
{
    /// <summary>
    /// Player的动画根运动，RootMotion是需要传递给服务端的(比较重要),客户端倒是不需要
    /// ！网络同步场景，比如服务器需要基于根运动计算角色权威位置，再同步给客户端
    /// </summary>
#if UNITY_SERVER || UNITY_EDITOR
    [SerializeField] private Animator animator;
    public event Action<Vector3, Quaternion> rootMotionAction; //委托（回调函数）,传递根运动给外部
    private void OnAnimatorMove()
    {
        /// 通过 Animator 获取动画根运动的帧数据（位置和旋转变化），并通过委托机制允许外部代码处理这些数据
        rootMotionAction?.Invoke(animator.deltaPosition, animator.deltaRotation);

    }
#endif

    #region 动画事件
    public event Action footStepAction;
    private void FootStep()
    {
        footStepAction?.Invoke();
    }
    /// <summary>
    /// 把原来使用的Action加了一个event。简化一下使用，这样就不用set很多方法了，比如下面，就去掉了，要不然以后再加一些Action会显得内容乱，这样就+=注册就行了
    /// </summary>
    //private Action jumpStartEndAction;
    //public void SetJumpStartEndAction(Action jumpStartEndAction)  //相当于event的 += 创建
    //{
    //    this.jumpStartEndAction = jumpStartEndAction;
    //}
    //public void CleanJumpStartEndAction(Action jumpStartEndAction)    //相当于event的 -=  清理
    //{
    //    this.jumpStartEndAction = null;
    //}
    public event Action startSkillHitAction;
    public event Action stopSkillHitAction;

    /// <summary>
    /// 技能攻击判定开始：攻击动作的Event，函数名称必须设置的与动画里的Event一模一样
    /// </summary>
    private void StartSkillHit()
    {
        startSkillHitAction?.Invoke();
    }
    private void StopSkillHit()
    {
        stopSkillHitAction?.Invoke();
    }
    #endregion


    #region Editor编辑器
    //添加手动启动器，到PlayerController脚本
    //完全通过代码取代，不使用Animator的话，容易跳帧。要想有自然的过渡，还是得用Animator的连线的机制功能。
    [Button, ContextMenu(nameof(SetAnimatorSettings))]
    public void SetAnimatorSettings()
    {
        //是可以强转过来的，因为他可以从继承关系上拿到，AnimatorController点进去看，可以知道是runtimeAnimatorController他的子类
        AnimatorController animatorController = (AnimatorController)animator.runtimeAnimatorController;
        animatorController.parameters = null;
        //遍历Animator动画控制器里的所有的状态：并附上过渡
        AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
        stateMachine.anyStateTransitions = null;
        foreach (ChildAnimatorState state in stateMachine.states)
        {
            string triggerName = state.state.name;
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                name = state.state.name,
                type = AnimatorControllerParameterType.Trigger
            };
            animatorController.AddParameter(parameter);
            //创建从 Any State 到每一个动画的连线，并附加上动画名称的过渡Trigger
            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(state.state);
            transition.AddCondition(AnimatorConditionMode.If, 0.0f, triggerName);
        }
        UnityEditor.EditorUtility.SetDirty(animatorController);
        UnityEditor.AssetDatabase.SaveAssetIfDirty(animatorController);
    }
    #endregion
}