using JKFrame;
using System;
using UnityEngine;
using UnityEngine.AI;

public class MonsterServerController : CharacterServerControllerBase<MonsterController>,IMonsterServerController,IStateMachineOwner
{
    public NavMeshAgent navMeshAgent { get; private set; }
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

    private void ChangeState(MonsterState newState)
    {
        mainController.currentState.Value = newState;
        switch (newState)
        {
            case MonsterState.Idle:
                stateMachine.ChangeState<MonsterIdleState>();
                break;
        }
    }

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


}
