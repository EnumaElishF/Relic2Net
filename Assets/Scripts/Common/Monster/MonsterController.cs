using JKFrame;

public class MonsterController : CharacterControllerBase<MonsterView, IMonsterClientController, IMonsterServerController>
{
    public MonsterConfig monsterConfig;
    public NetVariable<MonsterState> currentState = new NetVariable<MonsterState>(MonsterState.None);
    public override void OnNetworkSpawn()
    {
#if !UNITY_SERVER || UNITY_EDITOR
        if (IsClient)
        {
            //初始化生成MonsterrController，要优先于clientController调用之前
            EventSystem.TypeEventTrigger(new SpawnMonsterEvent { newMonster = this });

            //那么上面这里有触发事件了，谁来监听这个事件呢？SpawnMonsterEvent，需要考虑好放那个地方
        }
#endif
        base.OnNetworkSpawn();
    }
}
