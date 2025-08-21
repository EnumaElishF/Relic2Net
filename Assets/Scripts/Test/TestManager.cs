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
            // �ӳ�
            GUILayout.Label("Delay:" + ClientRTTInfo.Instance.rttMs);
            //��ǰ����
            GUILayout.Label("Position:" + Player.localPlayer.transform.position);
            // ����˶������� :��ȡ�������˶�����������
            if (NetManager.Instance.SpawnManager.OwnershipToObjectsTable.TryGetValue(NetManager.ServerClientId,out Dictionary<ulong,NetworkObject> temp))
            {
                GUILayout.Label("ServerObjects:" + temp.Count);
            }
            //�����ͻ��˶�������
            int clientObjects = 0;
            foreach(KeyValuePair<ulong, Dictionary<ulong, NetworkObject>> item in NetManager.Instance.SpawnManager.OwnershipToObjectsTable)
            {
                if(item.Key!=NetManager.ServerClientId && item.Key != NetManager.Instance.LocalClientId)
                {
                    // ����Ȳ��� ������id��Ҳ���� ���ؿͻ��� �Լ���id
                    clientObjects += item.Value.Count; //������֪���������ͻ����ж��ٸ�
                }
            }
            GUILayout.Label("Other ClientObjects:" + clientObjects);

        }
#endif
    }
}
