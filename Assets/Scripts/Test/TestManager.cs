using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public GameObject serverTestObjectPrefab;
    private void Start()
    {
#if UNITYSERVER || SERVER_EDITOR_TEST
        if (NetManager.Instance.IsServer)
        {
            NetManager.Instance.SpawnObject(NetManager.ServerClientId, serverTestObjectPrefab, Vector3.zero);
        }
#endif
    }

    private void OnGUI()
    {
#if SERVER_EDITOR_TEST
        if(Player.localPlayer != null)
        {
            // 延迟
            GUILayout.Label("Delay:" + ClientRTTInfo.Instance.rttMs);
            //当前坐标
            GUILayout.Label("Position:" + Player.localPlayer.transform.position);
            // 服务端对象数量 :获取网络服务端对象数量方法
            if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(NetManager.ServerClientId,out Dictionary<ulong,NetworkObject> temp))
            {
                GUILayout.Label("ServerObjects:" + temp.Count);
            }
            //其他客户端对象数量
            int clientObjects = 0;
            foreach(KeyValuePair<ulong, Dictionary<ulong, NetworkObject>> item in NetManager.Instance.SpawnManager.OwnershipToObjectsTable)
            {
                if(item.Key!=NetManager.ServerClientId && item.Key != NetManager.Instance.LocalClientId)
                {
                    // 如果既不是 服务器id，也不是 本地客户端 自己的id
                    clientObjects += item.Value.Count; //这样就知道了其他客户端有多少个
                }
            }
            GUILayout.Label("Other ClientObjects:" + clientObjects);

        }
#endif
    }
}
