using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Unity.Netcode;
public class AOIManager : SingletonMono<AOIManager>
{
    [SerializeField] private float chunkSize = 50; //最远可以看到的其他玩家或者服务端对象
    [SerializeField] private int visualChunkRange = 1;//如果是1，就是刚好周围的一圈：九宫格
    // <chunkCoord,clinetIDs>   记录这一块有哪些客户端玩家
    private Dictionary<Vector2Int, HashSet<ulong>> chunkClientDic = new Dictionary<Vector2Int, HashSet<ulong>>();
    // <chunkCoord,serverObjectIDs>    记录这一块有哪些服务端对象
    private Dictionary<Vector2Int, HashSet<NetworkObject>> chunkServerObjectDic = new Dictionary<Vector2Int, HashSet<NetworkObject>>();
    
    //客户端互相可见
    private void ClientMutualShow(ulong clientA,ulong clientB)
    {
        if (clientA == clientB) return;
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorObjectDic))
        {
            // A可见B
            foreach (NetworkObject aItem in aNetWorObjectDic.Values)
            {
                if (!aItem.IsNetworkVisibleTo(clientB)) aItem.NetworkShow(clientB);//如果不可见就展示
            }
            // B可见A
            foreach (NetworkObject bItem in bNetWorObjectDic.Values)
            {
                if (!bItem.IsNetworkVisibleTo(clientA)) bItem.NetworkShow(clientA);
            }
        }
    }
    //客户端互相不可见
    private void ClientMutualHide(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) return;
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorObjectDic))
        {
            // A可见B
            foreach (NetworkObject aItem in aNetWorObjectDic.Values)
            {
                if (aItem.IsNetworkVisibleTo(clientB)) aItem.NetworkHide(clientB); //如果可见就隐藏
            }
            // B可见A
            foreach (NetworkObject bItem in bNetWorObjectDic.Values)
            {
                if (bItem.IsNetworkVisibleTo(clientA)) bItem.NetworkHide(clientA);
            }
        }
    }


}
