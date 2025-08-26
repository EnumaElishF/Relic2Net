using Unity.Netcode;
using UnityEngine;

//Test��ʱ�ű��������ɾ��
public class ServerTestObject : NetworkBehaviour
{
    public float moveSpeed;
    public static ServerTestObject Instance;
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
#if UNITYSERVER || SERVER_EDITOR_TEST
        Instance = this;
        AOIManager.Instance.InitServerObject(NetworkObject, Vector2Int.zero);
#endif
     
    }
    private void Update()
    {
#if UNITYSERVER || SERVER_EDITOR_TEST
        if (IsServer)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(h, 0, v).normalized;
            HandleMovement(inputDir);
        }
#endif
    }
    private void HandleMovement(Vector3 inputDir)
    {

        Vector2Int oldCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        //���߷���ˣ����������
        transform.Translate(Time.deltaTime * moveSpeed * inputDir);
        Vector2Int newCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        if (newCoord != oldCoord) //�����˵�ͼ�������仯
        {
            AOIManager.Instance.UpdateServerObjectChunkCoord(NetworkObject, oldCoord, newCoord);

        }

    }
}
