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
        if (timer <= 0 || (!serverController.navMeshAgent.isPathStale && serverController.navMeshAgent.remainingDistance<=1f))
        {
            serverController.ChangeState(MonsterState.Idle);
        }
    }
    public override void Exit()
    {
        serverController.StopMove();
    }
}