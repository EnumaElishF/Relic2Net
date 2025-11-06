using Unity.Netcode;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public GameObject[] monsterPrefab;
    private void Start()
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsClient) return;
#endif
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
            serverController.Init();
            monsterObject.SpawnWithOwnership(NetManager.ServerClientId);
        }
    }

}
