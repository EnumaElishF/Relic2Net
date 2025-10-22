using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ClientTestManager : MonoBehaviour
{
    //public GameObject serverTestObjectPrefab;
    //public NetworkObject testInstance;


    private void OnGUI()
    {
        //#if UNITY_SERVER || UNITY_EDITOR
        //        //服务器
        //        if (NetManager.Instance.IsServer)
        //        {
        //            if (ServerTestObject.Instance != null)
        //            {
        //                //Debug.Log("服务器对象Position的GUI展示问题");
        //                GUILayout.Label("Server Object Position:" + ServerTestObject.Instance.transform.position);
        //            }
        //            if(testInstance == null && Input.GetKeyDown(KeyCode.E))
        //            {
        //                testInstance = NetManager.Instance.SpawnObject(NetManager.ServerClientId, serverTestObjectPrefab, Vector3.zero, Quaternion.identity);
        //            }
        //            if (testInstance != null && Input.GetKeyDown(KeyCode.F))
        //            {
        //                NetManager.Instance.DestroyObject(testInstance);
        //                testInstance = null;
        //            }
        //        }

        //#endif

        //客户端
        if (PlayerManager.Instance != null && PlayerManager.Instance.localPlayer != null)
        {
            // 延迟
            GUILayout.Label("延迟Delay:" + ClientRTTInfo.Instance.rttMs);
            //当前坐标
            GUILayout.Label("Player Position:" + PlayerManager.Instance.localPlayer.transform.position);
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

    }

}
