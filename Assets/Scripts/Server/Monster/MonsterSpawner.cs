using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject[] monsterPrefabs;
    public float patrolRange = 10;
    public float interval = 10; //隔多少秒生成一次
    private float halfPatrolRange;
    private MonsterServerController[] monsters;
    private float timer;
    public void Init()
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsClient) return;
#endif
        timer = interval;
        monsters = new MonsterServerController[monsterPrefabs.Length];
        halfPatrolRange = patrolRange / 2f;
        for(int i = 0; i < monsterPrefabs.Length; i++)
        {
            Spawn(i);
        }
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsClient) return;
#endif
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = interval;
            for (int i = 0; i < monsters.Length; i++)
            {
                if (monsters[i] == null)
                {
                    Spawn(i);
                    return;
                }
            }
        }
    }
    private void Spawn(int index)
    {
        NetworkObject monsterObject = NetManager.Instance.SpawnObjectNoShow(NetManager.ServerClientId, monsterPrefabs[index], GetPatrolPoint(), Quaternion.identity);
        //初始化怪物的服务端组件
        if (!monsterObject.TryGetComponent(out MonsterServerController serverController))
        {
            serverController = monsterObject.gameObject.AddComponent<MonsterServerController>();
            serverController.FirstInit(monsterObject.GetComponent<MonsterController>());
        }
        serverController.SetMonsterSpawner(this,index);
        serverController.Init();
        monsterObject.SpawnWithOwnership(NetManager.ServerClientId);
        monsters[index] = serverController;
    }

    public Vector3 GetPatrolPoint()
    {
        //随机生成初始候选巡逻点
        Vector3 point = transform.position + new Vector3(Random.Range(-halfPatrolRange, halfPatrolRange), 0, Random.Range(-halfPatrolRange, halfPatrolRange));
        //验证候选点是否在可行走区域（NavMesh 校验） 10米范围
        if (NavMesh.SamplePosition(point, out NavMeshHit hitInfo, 10f, NavMesh.AllAreas))
        {
            return hitInfo.position;
        }
        else
        {
            return transform.position;
        }
    }

    public void OnMonsterDie(int index)
    {
        monsters[index] = null;
    }
}
