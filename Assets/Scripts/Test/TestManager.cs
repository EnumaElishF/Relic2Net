using System.Collections;
using System.Collections.Generic;
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
}
