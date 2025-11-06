using JKFrame;

public class MonsterClientManager : SingletonMono<MonsterClientManager>
{
    public void Init()
    {
        //监听
        EventSystem.AddTypeEventListener<SpawnMonsterEvent>(OnSpawnMonsterEvent);

    }

    private void OnSpawnMonsterEvent(SpawnMonsterEvent arg)
    {
        if (!arg.newMonster.TryGetComponent(out MonsterClientController clientController))
        {
            clientController = arg.newMonster.gameObject.AddComponent<MonsterClientController>();
            clientController.FirstInit(arg.newMonster);
        }
        clientController.Init();

    }
}