using UnityEngine;
/// <summary>
/// 击退后退的话，需要考虑一个击退速度的问题
/// </summary>
public class MonsterDamageState : MonsterStateBase
{
    private float repelTimer;//击退计时器
    private float repelSpeed;//击退速度
    private Vector3 repelDir;// 击退方向
    public override void Enter()
    {
        serverController.PlayAnimation("Damage");
    }
    public void SetAttackData(AttackData attackData)
    {
        repelTimer = attackData.repelTime;
        repelSpeed = attackData.repelTime>0 ? attackData.repelDistance / attackData.repelTime : 0;
        repelDir = serverController.transform.position - attackData.sourcePosition; //获得从源头打向攻击目标的向量
        repelDir.Normalize();
    }
    public override void Update()
    {
        repelTimer -= Time.deltaTime;
        //击退完成了，并且动画也结束了
        //0.95f 算是个人觉得的通用经验选择：既保证受击动画基本播完（玩家视觉上已经看到受击动作结束），又不会因动画切换、帧率问题漏判
        if (repelTimer <= 0 && (serverController.CheckAnimationState("Damage", out float normalizedTime) && normalizedTime >= 0.95f))// 击退倒计时结束，
        {
            if (serverController.CheckTargetPlayer())
            {
                serverController.ChangeState(MonsterState.Pursuit);
            }
            else
            {
                serverController.ChangeState(MonsterState.Patrol);
            }
        }else if (repelTimer > 0)
        {
            //如果击退还没有完成就继续向后退
            Vector3 motion = repelSpeed * repelDir;
            motion.y = -5f;
            serverController.characterController.Move(motion * Time.deltaTime);
        }


    }
}