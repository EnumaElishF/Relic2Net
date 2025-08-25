using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
using Unity.Netcode;
public class AOIManager : SingletonMono<AOIManager>
{
    [SerializeField] private float chunkSize = 50; //��Զ���Կ�����������һ��߷���˶���
    [SerializeField] private int visualChunkRange = 1;//�����1�����Ǹպ���Χ��һȦ���Ź���
    // <chunkCoord,clinetIDs>   ��¼��һ������Щ�ͻ������
    private Dictionary<Vector2Int, HashSet<ulong>> chunkClientDic = new Dictionary<Vector2Int, HashSet<ulong>>();
    // <chunkCoord,serverObjectIDs>    ��¼��һ������Щ����˶���
    private Dictionary<Vector2Int, HashSet<NetworkObject>> chunkServerObjectDic = new Dictionary<Vector2Int, HashSet<NetworkObject>>();
    
    //�ͻ��˻���ɼ�
    private void ClientMutualShow(ulong clientA,ulong clientB)
    {
        if (clientA == clientB) return;
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorObjectDic))
        {
            // A�ɼ�B
            foreach (NetworkObject aItem in aNetWorObjectDic.Values)
            {
                if (!aItem.IsNetworkVisibleTo(clientB)) aItem.NetworkShow(clientB);//������ɼ���չʾ
            }
            // B�ɼ�A
            foreach (NetworkObject bItem in bNetWorObjectDic.Values)
            {
                if (!bItem.IsNetworkVisibleTo(clientA)) bItem.NetworkShow(clientA);
            }
        }
    }
    //�ͻ��˻��಻�ɼ�
    private void ClientMutualHide(ulong clientA, ulong clientB)
    {
        if (clientA == clientB) return;
        if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientA, out Dictionary<ulong, NetworkObject> aNetWorObjectDic)
            && NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(clientB, out Dictionary<ulong, NetworkObject> bNetWorObjectDic))
        {
            // A�ɼ�B
            foreach (NetworkObject aItem in aNetWorObjectDic.Values)
            {
                if (aItem.IsNetworkVisibleTo(clientB)) aItem.NetworkHide(clientB); //����ɼ�������
            }
            // B�ɼ�A
            foreach (NetworkObject bItem in bNetWorObjectDic.Values)
            {
                if (bItem.IsNetworkVisibleTo(clientA)) bItem.NetworkHide(clientA);
            }
        }
    }


}
