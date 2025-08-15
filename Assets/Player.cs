using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<float> moveSpeed;   //���������ֵ���ͣ������ǽṹ��
#if UNITY_EDITOR
    public bool test;
#endif

    public override void OnNetworkSpawn()
    {
        moveSpeed = new NetworkVariable<float>(1);
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
        if (IsServer)
        {
            //IsServer ��һ������ֵ�������жϵ�ǰ���еĳ���ʵ���Ƿ��Ƿ����������� ��������Host�������� ��Ϊ Host ͬʱ���ݷ������Ϳͻ��˵Ľ�ɫ����
            if (Input.GetKeyDown(KeyCode.I))
            {
                TestClientRpc();
            }
        }
#if UNITY_EDITOR
        if (IsServer && test)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                moveSpeed.Value += 0.5f;
            }
        }
#endif
}

//�൱�ڵ��÷����������ı���
//---Ҳ����˵���������������жϺ󣬰����յ������ƶ������˷�������
//�����Rpc���õĵط���
//�����moveSpeed���㱾�ؿͻ������ף��޸������ݣ����Ƿ������ʹ�õ�ʱ���õ��Ƿ���˵�moveSpeed�����ﵽ�˷�����Ч����
[ServerRpc(RequireOwnership =false)]
    private void HandleMovementServerRpc(Vector3 inputDir)
    {
        //���߷���ˣ����������
        transform.Translate(Time.deltaTime * moveSpeed.Value * inputDir);
    }

    [ClientRpc]
    private void TestClientRpc()
    {
        //��һ��ʵ������Ϸ��Ʒ�Ĳ���
        GameObject.CreatePrimitive(PrimitiveType.Capsule).transform.position  =transform.position;
    }
}
