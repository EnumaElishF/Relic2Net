
using UnityEngine;

public class PlayerAttackState : PlayerStateBase
{
    private int attackIndex = -1;
    private SkillConfig skillConfig;
    /// <summary>
    /// 管理攻击动画事件
    /// </summary>
    public override void Enter()
    {
        Player_View view = mainController.view;
        view.startSkillHitAction += View_startSkillHitAction;
        view.stopSkillHitAction += View_stopSkillHitAction;
        view.rootMotionAction += OnRootMotion;
        StartAttack();
    }

    public override void Exit()
    {
        Player_View view = mainController.view;
        view.startSkillHitAction -= View_startSkillHitAction;
        view.stopSkillHitAction -= View_stopSkillHitAction;
        view.rootMotionAction -= OnRootMotion;
    }
    public override void Update()
    {
        if(CheckAnimationState(skillConfig.animationName,out float time))
        {
            if(time >= skillConfig.switchTime && serverController.inputData.attack)
            {
                StartAttack();
            }
            else if (time >= skillConfig.endTime)
            {
                //结束动画
                serverController.ChangeState(serverController.inputData.moveDir == Vector3.zero ? PlayerState.Idle : PlayerState.Move);
            }
        }
    }
    private void StartAttack()
    {
        //前摇部分不能切换
        attackIndex += 1;
        //不管有几段攻击，只要超出配置表的技能数量，就会重新循环
        if (attackIndex >= mainController.skillConfigList.Count) attackIndex = 0;
        skillConfig = mainController.skillConfigList[attackIndex];
        //播放动画，技能配置表的名称
        serverController.PlayAnimation(skillConfig.animationName);
    }
    private void View_startSkillHitAction()
    {
        
    }
    private void View_stopSkillHitAction()
    {
        
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        serverController.animator.speed = serverController.rootMotionMoveSpeedMultiply;
        deltaPosition.y -= 9.8f * Time.deltaTime;  //模拟重力
        serverController.characterController.Move(deltaPosition);
        //更新AOI :因为有位移，就要更新AOI
        if (deltaPosition != Vector3.zero)
        {
            serverController.UpdateAOICoord();
        }
    }
}
