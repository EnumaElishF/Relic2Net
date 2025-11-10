using UnityEngine;

public class MonsterIdleState : MonsterStateBase
{
    private float timer;//计时器
    //private float time;//时间
    public override void Enter()
    {
        serverController.PlayAnimation("Idle");
        timer = Random.Range(config.maxIdleTime / 2f, config.maxIdleTime);//待机2.5秒至5秒之间
    }
    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)//倒计时
        {
            serverController.ChangeState(MonsterState.Patrol);
        }
        serverController.RecoverHP();
        //搜索玩家
        PlayerServerController player = serverController.SearchPlayer();
        if (player != null)
        {
            serverController.SetTargetPlayer(player);
            serverController.ChangeState(MonsterState.Pursuit);
        }
    }
}