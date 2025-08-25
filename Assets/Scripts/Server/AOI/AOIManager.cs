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
    /// <summary>
    /// ���������AOI��ͼ�ϵ�����   (���chunk��)
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="oldCoord">"coord" ������ "coordinate" ����д</param>
    /// <param name="newCoord"></param>
    public void UpdateClientChunkCoord(ulong clientID,Vector2Int oldCoord,Vector2Int newCoord)
    {
        if (oldCoord == newCoord) return;
        // �Ӿɵĵ�ͼ�����Ƴ�
        RemoveClient(clientID, oldCoord);

        //�ж��Ƿ���ͼ���ƶ�  (���ͼ��Ļ��������ƴ��͵����)
        if (Vector2Int.Distance(oldCoord, newCoord) > 1.5f) //�������������ƶ��ļ��޾���,�����Ǵ������ʵ�λ��
        {
            //����ƶ�
            //�������ӵ�б�Ƕ�λ�ƿ϶���С��1.5f�ģ������˾�˵���������ӵļ��޾��� ��2 = 1.414
            for (int x = - visualChunkRange; x<=visualChunkRange; x++) //-1,0,1�ľŹ���
            {
                for(int y = -visualChunkRange; y <= visualChunkRange; y++)
                {
                    //��Ȼ�����㷨��ʱ�临�ӶȱȽϴ󣬵�����Ϊ��ԵľŸ�����+�ͻ����������ƣ�ʵ���ϲ���ܴ�
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + x, oldCoord.y + y);
                    Vector2Int showChunkCoord = new Vector2Int(oldCoord.x + x, oldCoord.y + y);
                    ShowAndHideForChunkClients(clientID, hideChunkCoord,showChunkCoord);
                }
            }
        }
        else //����һ�����ӵ��ƶ�����
        {
            //�ǿ���ƶ��������������ң��Լ����б�����ƶ���ע��б�������ᱻ�ֽ��������Ϊ��ϵ����ϣ��������)
            // �ϣ��ɵ�������һ�����أ��µ�����һ����ʾ
            if (newCoord.y > oldCoord.y)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y - visualChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y + visualChunkRange);
                    ShowAndHideForChunkClients(clientID, hideChunkCoord, showChunkCoord);
                }
            }
            // �£��ɵ�������һ����ʾ���µ�����һ������
            else if (newCoord.y < oldCoord.y)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + i, oldCoord.y + visualChunkRange);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + i, newCoord.y - visualChunkRange);
                    ShowAndHideForChunkClients(clientID, hideChunkCoord, showChunkCoord);
                }
            }

            // �󣬾ɵ����ұ���һ�����أ��µ������һ����ʾ
            if (newCoord.x < oldCoord.x)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x + visualChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x - visualChunkRange, newCoord.y + i);
                    ShowAndHideForChunkClients(clientID, hideChunkCoord, showChunkCoord);
                }
            }
            // �ң��ɵ����ұ���һ����ʾ���µ������һ������
            else if (newCoord.x > oldCoord.x)
            {
                for (int i = -visualChunkRange; i <= visualChunkRange; i++)
                {
                    Vector2Int hideChunkCoord = new Vector2Int(oldCoord.x - visualChunkRange, oldCoord.y + i);
                    Vector2Int showChunkCoord = new Vector2Int(newCoord.x + visualChunkRange, newCoord.y + i);
                    ShowAndHideForChunkClients(clientID, hideChunkCoord, showChunkCoord);
                }
            }
        }

        //�ѿͻ��˼��뵱ǰ�µĿ�----�ȴӾɵĵ�ͼ�����Ƴ����ְѿͻ��˼��뵱ǰ�µĿ顣�������ڲ�ķ���Ҳ��֤���Լ�������Լ������
        if (!chunkClientDic.TryGetValue(newCoord,out HashSet<ulong> newCoordClientIDs))
        {
            newCoordClientIDs = ResSystem.GetOrNew<HashSet<ulong>>();
            chunkClientDic.Add(newCoord,newCoordClientIDs);
        }
        newCoordClientIDs.Add(clientID);
    }

    /// <summary>
    /// Ϊĳ����ͼ���µ�ȫ���ͻ��ˣ���ʾ������ĳ���ͻ���
    /// </summary>
    private void ShowAndHideForChunkClients(ulong clientID,Vector2Int hideChunkCoord,Vector2Int showChunkCoord)
    {
        ShowClientForChunkClients(clientID, showChunkCoord);
        HideClientForChunkClients(clientID, hideChunkCoord);
    }
    // ĳ���ͻ��˺�ĳ������Ŀͻ�����ȫ������ �ɼ�
    private void ShowClientForChunkClients(ulong clientID,Vector2Int chunkCoord)
    {
        if(chunkClientDic.TryGetValue(chunkCoord,out HashSet<ulong> clientIDs))
        {
            foreach(ulong newClientID in clientIDs)
            {
                ClientMutualShow(clientID, newClientID);
            }
        }
    }
    // ĳ���ͻ��˺�ĳ������Ŀͻ�����ȫ������ ���ɼ�
    private void HideClientForChunkClients(ulong clientID, Vector2Int chunkCoord)
    {
        if (chunkClientDic.TryGetValue(chunkCoord, out HashSet<ulong> clientIDs))
        {
            foreach (ulong newClientID in clientIDs)
            {
                ClientMutualHide(clientID, newClientID);
            }
        }
    }

    public void RemoveClient(ulong clientID,Vector2Int coord)
    {
        if(chunkClientDic.TryGetValue(coord,out HashSet<ulong> clientIDs)) //�õ�ulong��id
        {
            //�����ǰ������û����ң������������
            if(clientIDs.Remove(clientID) && clientIDs.Count == 0)
            {
                clientIDs.ObjectPushPool();
                chunkClientDic.Remove(coord);
            }
        }
    }
    
    /// <summary>
    /// �ͻ��˻���ɼ�
    /// </summary>
    /// <param name="clientA"></param>
    /// <param name="clientB"></param>
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
    /// <summary>
    /// �ͻ��˻��಻�ɼ�
    /// </summary>
    /// <param name="clientA"></param>
    /// <param name="clientB"></param>
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

    public Vector2Int GetCoordByWorldPostion(Vector3 worldPostion)
    {
        return new Vector2Int((int)(worldPostion.x / chunkSize), (int)(worldPostion.z / chunkSize));
    }


}
