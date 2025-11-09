using UnityEngine;

public class MonsterPatrolState : MonsterStateBase
{
    private float timer;//计时器
    //private float time;//时间
    public override void Enter()
    {
        serverController.PlayAnimation("Move");
        timer = Random.Range(config.maxPatrolTime / 2f, config.maxPatrolTime);//巡逻。秒之间随机
        Vector3 point = serverController.GetPatrolPoint();
        serverController.StartMove();
        serverController.navMeshAgent.SetDestination(point);
    }
    public override void Update()
    {
        timer -= Time.deltaTime;
        //倒计时结束 或 serverController.navMeshAgent.isPathStale路径非有效的,-有效的路径&&剩余距离小于1
        //目标点的(!serverController.navMeshAgent.isPathStale && serverController.navMeshAgent.remainingDistance<=1f)
        if (timer <= 0 || (!serverController.navMeshAgent.isPathStale && serverController.navMeshAgent.remainingDistance<=1f))
        {
            serverController.ChangeState(MonsterState.Idle);
            return;
        }
        //搜索玩家
        PlayerServerController player = serverController.SearchPlayer();
        if (player != null)
        {
            serverController.ChangeState(MonsterState.Pursuit);
            ((MonsterPursuitState)stateMachine.currStateObj).SetTarget(player);
        }
    }
    public override void Exit()
    {
        serverController.StopMove();
    }

}