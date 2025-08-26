using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    //�����ң�����Playerû�е���
    public static Player localPlayer { get; private set; }

    //public NetworkVariable<float> moveSpeed;   //���������ֵ���ͣ������ǽṹ��
    public float moveSpeed = 3;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
#if !UNITY_SERVER
        if(IsOwner&& IsClient)
        {
            //�ͻ��˿��ٷ��ʵ� ���Լ����Ƶ���Ҷ���
            localPlayer = this;
        }
#endif

#if UNITY_SERVER || SERVER_EDITOR_TEST
        if (IsServer)
        {
            AOIManager.Instance.InitClient(OwnerClientId, Vector2Int.zero); //��ұ��˵���Ϸ�����id
        }

#endif

    }

    void Update()
    {
        if (!IsSpawned) return; //�����δ���ѣ��Ͳ�ִ�������߼�

        if (IsOwner)
        {
            //IsOwner ��һ������ֵ�������жϵ�ǰ���ؿͻ����Ƿ��Ǹ� Network Object �� �������ߡ���Owner����
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(h, 0, v).normalized;
            HandleMovementServerRpc(inputDir);
        }

}

//�൱�ڵ��÷����������ı���
//---Ҳ����˵���������������жϺ󣬰����յ������ƶ������˷�������
//�����Rpc���õĵط���
//�����moveSpeed���㱾�ؿͻ������ף��޸������ݣ����Ƿ������ʹ�õ�ʱ���õ��Ƿ���˵�moveSpeed�����ﵽ�˷�����Ч����
[ServerRpc(RequireOwnership =false)]  //ֻ�ᱻ����˵��õķ���
    private void HandleMovementServerRpc(Vector3 inputDir)
    {
        Vector2Int oldCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        //���߷���ˣ����������
        transform.Translate(Time.deltaTime * moveSpeed * inputDir);

        Vector2Int newCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        if (newCoord != oldCoord) //�����˵�ͼ�������仯
        {
            AOIManager.Instance.UpdateClientChunkCoord(OwnerClientId, oldCoord, newCoord);

        }
 
    }


}
