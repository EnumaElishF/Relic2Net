using UnityEngine;

public class MonsterAttackState : MonsterStateBase
{
    private SkillConfig skillConfig;
    public override void Enter()
    {
        MonsterView view = mainController.View;
        view.startSkillHitAction += View_startSkillHitAction;
        view.stopSkillHitAction += View_stopSkillHitAction;
        StartAttack();
    }
    public override void Exit()
    {
        MonsterView view = mainController.View;
        view.startSkillHitAction -= View_startSkillHitAction;
        view.stopSkillHitAction -= View_stopSkillHitAction;
        View_stopSkillHitAction();//退出武器攻击状态，再加一次退出，更安全一些，因为有可能状态被别人打断，导致武器一直是攻击状态。
    }
    public override void Update()
    {
        if (serverController.CheckAnimationState(skillConfig.animationName, out float normalizedTime))
        {
            if(normalizedTime < skillConfig.rotateNormalizedTime)
            {
                if (serverController.CheckTargetPlayer())
                {
                    Vector3 dir = serverController.targetPlayer.transform.position - serverController.transform.position;
                    serverController.transform.rotation = Quaternion.RotateTowards(mainController.View.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * MonsterServerController.attackRotateSpeed);

                }
            }
            else if(normalizedTime >= skillConfig.endNormalizedTime)
            {
                serverController.ChangeState(MonsterState.Pursuit);
            }
        }
    }
    private void StartAttack()
    {
        serverController.OnAttack();
        int attackIndex = Random.Range(0, mainController.skillConfigList.Count);
        //不管有几段攻击，只要超出配置表的技能数量，就会重新循环
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
    public void OnHit(IHitTarget target, Vector3 point)
    {
        //服务端只处理伤害，AI的状态逻辑，表现层应该发给客户端去做
        AttackData attackData = new AttackData
        {
            attackValue = skillConfig.attackValueMultiple * mainController.monsterConfig.attackValue,//技能攻击力系数*武器攻击力
            repelDistance = skillConfig.repelDistance,
            repelTime = skillConfig.repelTime,
            sourcePosition = serverController.transform.position
        };
        //通知客户端播放效果
        mainController.PlaySkillHitEffectClientRpc(point);
        //通知玩家受伤
        target.BeHit(attackData);
    }

}