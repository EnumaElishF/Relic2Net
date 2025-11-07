using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterSpawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public GameObject[] monsterPrefab;
    public float patrolRange = 10;
    private float halfPatrolRange;
    public void Init()
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsClient) return;
#endif
        halfPatrolRange = patrolRange / 2f;
        for(int i = 0; i < monsterPrefab.Length; i++)
        {
            Transform point = spawnPoint[i];
            NetworkObject monsterObject = NetManager.Instance.SpawnObjectNoShow(NetManager.ServerClientId, monsterPrefab[i], point.position, point.rotation);
            //初始化怪物的服务端组件
            if(!monsterObject.TryGetComponent(out MonsterServerController serverController))
            {
                serverController = monsterObject.gameObject.AddComponent<MonsterServerController>();
                serverController.FirstInit(monsterObject.GetComponent<MonsterController>());
            }
            serverController.SetMonsterSpawner(this);
            serverController.Init();
            monsterObject.SpawnWithOwnership(NetManager.ServerClientId);
        }
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
}
