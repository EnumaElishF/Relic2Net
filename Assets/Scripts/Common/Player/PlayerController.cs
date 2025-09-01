using JKFrame;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
#if !UNITY_SERVER
    public Transform cameraLookatTarget;
    public Transform cameraFollowTarget;
#endif
    public Transform Transform { get => transform; }
    //�����ң�����Playerû�е���
    //public NetworkVariable<float> moveSpeed;   //���������ֵ���ͣ������ǽṹ��
    public float moveSpeed = 3;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
#if !UNITY_SERVER || UNITY_EDITOR
		if (IsOwner&& IsClient)
        {
            //�ͻ��˿��ٷ��ʵ� ���Լ����Ƶ���Ҷ���
            //PlayerManager.Instance.InitLocalPlayer(this);  �滻Ϊ�·����¼�
            EventSystem.TypeEventTrigger(new InitLocalPlayerEvent { localPlayer = this });

        }
#endif

#if UNITY_SERVER || UNITY_EDITOR
        //if (IsServer)
        //{
        //    AOIManager.Instance.InitClient(OwnerClientId, Vector2Int.zero); //��ұ��˵���Ϸ�����id
        //}

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

#if UNITY_SERVER || UNITY_EDITOR
        //Vector2Int oldCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        ////���߷���ˣ����������
        //transform.Translate(Time.deltaTime * moveSpeed * inputDir);

        //Vector2Int newCoord = AOIManager.Instance.GetCoordByWorldPostion(transform.position);
        //if (newCoord != oldCoord) //�����˵�ͼ�������仯
        //{
        //    AOIManager.Instance.UpdateClientChunkCoord(OwnerClientId, oldCoord, newCoord);

        //}
#endif


    }


}
