using UnityEngine;

public class MonsterPursuitState : MonsterStateBase
{
    private float timer;//计时器
    public override void Enter()
    {
        serverController.PlayAnimation("Move");
        timer = config.pursuitTime;
        serverController.StartMove();
    }
    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            serverController.ChangeState(MonsterState.Patrol);
            return;
        }
        if (serverController.CheckTargetPlayer())
        {
            serverController.navMeshAgent.SetDestination(serverController.targetPlayer.transform.position);

        }
        else
        {
            serverController.ChangeState(MonsterState.Patrol);
        }
    }

    public override void Exit()
    {
        serverController.StopMove();
    }

}