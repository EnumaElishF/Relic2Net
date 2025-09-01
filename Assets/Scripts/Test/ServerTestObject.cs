using Unity.Netcode;
using UnityEngine;

//Test临时脚本，用完就删了.
public class ServerTestObject : NetworkBehaviour
{
    public float moveSpeed;
    public static ServerTestObject Instance;
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
#if UNITYSERVER || UNITY_EDITOR
        Instance = this;
        AOIManager.Instance.InitServerObject(NetworkObject, Vector2Int.zero);
#endif
     
    }
    private void Update()
    {
#if UNITYSERVER || UNITY_EDITOR
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

        //Vector2Int oldCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        ////告诉服务端，有这个事情
        //transform.Translate(Time.deltaTime * moveSpeed * inputDir);
        //Vector2Int newCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        //if (newCoord != oldCoord) //发生了地图块的坐标变化
        //{
        //    AOIManager.Instance.UpdateServerObjectChunkCoord(NetworkObject, oldCoord, newCoord);

        //}

    }
}
