using JKFrame;
using UnityEngine;
using UnityEngine.AI;

public class MonsterServerController : CharacterServerControllerBase<MonsterController>,IMonsterServerController,IStateMachineOwner,IHitTarget
{
    public const float recoverHPRate = 0.2f;
    public NavMeshAgent navMeshAgent { get; private set; }
    public CharacterController characterController { get; private set; }
    public MonsterSpawner monsterSpawner { get; private set; } //刷怪点
    public MonsterConfig monsterConfig { get => mainController.monsterConfig; }
    public override void FirstInit(MonsterController mainController)
    {
        base.FirstInit(mainController);
        navMeshAgent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        mainController.currentHp.Value = monsterConfig.maxHP;
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
            case MonsterState.Pursuit:
                stateMachine.ChangeState<MonsterPursuitState>();
                break;
            case MonsterState.Damage:
                stateMachine.ChangeState<MonsterDamageState>();
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

    #region 搜索玩家
    private float lastSearchPlayerTime; //不要每帧都搜索，我们做半秒搜索一次的时间，这样比较好
    private Collider[] hitCollider = new Collider[1]; // 索敌目标，目前暂时做一个索敌就行
    private const float searchPlayerInterval = 0.5f;

    public PlayerServerController SearchPlayer(bool checkTime = true)
    {
        if (checkTime)
        {
            if(Time.time - lastSearchPlayerTime < 0.5f)
            {
                return null;
            }
            lastSearchPlayerTime = Time.time;
        }
        int count = Physics.OverlapSphereNonAlloc(transform.position + new Vector3(0, 1, 0), monsterConfig.searchPlayerRange, hitCollider,ServerGlobal.Instance.PlayerLayerMask);
        if(count != 0)
        {
            return hitCollider[0].GetComponentInParent<PlayerServerController>();
        }
        return null;
    }


    #endregion
    #region 战斗
    public PlayerServerController targetPlayer { get; private set; }
    public void SetTargetPlayer(PlayerServerController targetPlayer)
    {
        this.targetPlayer = targetPlayer;
    }
    /// <summary>
    /// 检查目标玩家是否存活
    /// </summary>
    /// <returns></returns>
    public bool CheckTargetPlayer()
    {
        if (targetPlayer != null)
        {
            if (targetPlayer.Living)
            {
                return true;
            }
            else SetTargetPlayer(null);
        }
        return false;
    }

    public void BeHit(AttackData attackData)
    {
        //结算基本的伤害数值
        if (mainController.currentHp.Value <= 0) return;
        float hp = mainController.currentHp.Value;
        hp -= attackData.attackValue;
        if (hp < 0) hp = 0;
        mainController.currentHp.Value = hp;
        ChangeState(MonsterState.Damage);
        ((MonsterDamageState)stateMachine.currStateObj).SetAttackData(attackData);
    }
    /// <summary>
    /// 怪物脱战回血
    /// </summary>
    public void RecoverHP()
    {
        float hp = mainController.currentHp.Value;
        if (hp < monsterConfig.maxHP)
        {
            hp += monsterConfig.maxHP * recoverHPRate * Time.deltaTime;
            mainController.currentHp.Value = hp;
        }
    }
    #endregion
}
