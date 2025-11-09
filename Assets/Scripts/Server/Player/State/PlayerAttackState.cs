
using System;
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
        PlayerView view = mainController.View;
        view.startSkillHitAction += View_startSkillHitAction;
        view.stopSkillHitAction += View_stopSkillHitAction;
        view.rootMotionAction += OnRootMotion;
        StartAttack();
    }

    public override void Exit()
    {
        PlayerView view = mainController.View;
        view.startSkillHitAction -= View_startSkillHitAction;
        view.stopSkillHitAction -= View_stopSkillHitAction;
        view.rootMotionAction -= OnRootMotion;
        View_stopSkillHitAction();//退出武器攻击状态，再加一次退出，更安全一些，因为有可能状态被别人打断，导致武器一直是攻击状态。
    }
    public override void Update()
    {
        if(serverController.CheckAnimationState(skillConfig.animationName,out float normalizedTime))
        {
            if(serverController.inputData.moveDir != Vector3.zero && normalizedTime < skillConfig.rotateNormalizedTime)
            {
                mainController.View.transform.rotation = Quaternion.RotateTowards(mainController.View.transform.rotation, Quaternion.LookRotation(serverController.inputData.moveDir), Time.deltaTime * serverController.rotateSpeed);
            }
            if (normalizedTime >= skillConfig.switchNormalizedTime && serverController.inputData.attack)
            {
                StartAttack();
            }
            else if (normalizedTime >= skillConfig.endNormalizedTime)
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
        mainController.StartSkillClientRpc(attackIndex);
    }
    private void View_startSkillHitAction()
    {
        serverController.weapon.StartHit();
        mainController.StartSkillHitClientRpc();
    }
    private void View_stopSkillHitAction()
    {
        serverController.weapon.StopHit();
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

    public void OnHit(IHitTarget target, Vector3 point)
    {
        //服务端只处理伤害，AI的状态逻辑，表现层应该发给客户端去做
        AttackData attackData = new AttackData
        {
            attackValue = skillConfig.attackValueMultiple * serverController.weaponConfig.attackValue,//技能攻击力系数*武器攻击力
            repelDistance = skillConfig.repelDistance,
            repelTime = skillConfig.repelTime,
            sourcePosition = serverController.transform.position
        };
        //通知客户端播放效果
        mainController.PlaySkillHitEffectClientRpc(point);
        //通知怪物受伤
        target.BeHit(attackData);
    }
}
