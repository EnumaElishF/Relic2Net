using UnityEngine;

public class MonsterPursuitState : MonsterStateBase
{
    private float timer;//计时器
    private PlayerServerController targetPlayer;
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
        if (targetPlayer != null)
        {
            if (targetPlayer.Living)
            {
                serverController.navMeshAgent.SetDestination(targetPlayer.transform.position);
                return;
            }
            else targetPlayer = null;
        }
        if(targetPlayer == null)
        {
            serverController.ChangeState(MonsterState.Patrol);
        }

    }
    public override void Exit()
    {
        serverController.StopMove();
        targetPlayer = null;
    }
    public  void SetTarget(PlayerServerController player)
    {
        this.targetPlayer = player;
    }
}