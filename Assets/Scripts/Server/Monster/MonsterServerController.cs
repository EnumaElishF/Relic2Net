using JKFrame;
using UnityEngine;
using UnityEngine.AI;

public class MonsterServerController : CharacterServerControllerBase<MonsterController>,IMonsterServerController,IStateMachineOwner
{
    public NavMeshAgent navMeshAgent { get; private set; }
    public MonsterSpawner monsterSpawner { get; private set; } //刷怪点
    public override void FirstInit(MonsterController mainController)
    {
        base.FirstInit(mainController);
        navMeshAgent = GetComponent<NavMeshAgent>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ChangeState(MonsterState.Idle);
    }
    public void SetMonsterSpawner(MonsterSpawner monsterSpawner)
    {
        this.monsterSpawner = monsterSpawner;
    }
    public void ChangeState(MonsterState newState)
    {
        mainController.currentState.Value = newState;
        switch (newState)
        {
            case MonsterState.Idle:
                stateMachine.ChangeState<MonsterIdleState>();
                break;
            case MonsterState.Patrol:
                stateMachine.ChangeState<MonsterPatrolState>();
                break;
        }
    }
    #region AOI
    protected override void OnInitAOI()
    {
        AOIManager.Instance.InitServerObject(mainController.NetworkObject, currentAOICoord);
    }
    protected override void OnUpdateAOI(Vector2Int newCoord)
    {
        AOIManager.Instance.UpdateServerObjectChunkCoord(mainController.NetworkObject, currentAOICoord, newCoord);
    }
    protected override void OnRemoveAOI()
    {
        AOIManager.Instance.RemoveServerObject(mainController.NetworkObject, currentAOICoord);
    }
    #endregion

    #region 移动控制
    public void StartMove()
    {
        navMeshAgent.enabled = true;
    }
    public void StopMove()
    {
        navMeshAgent.enabled = false;
    }
    /// <summary>
    /// 获取导航点
    /// </summary>
    public Vector3 GetPatrolPoint()
    {
        return monsterSpawner.GetPatrolPoint();
    }
    #endregion

}
