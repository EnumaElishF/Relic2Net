using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    //public NetworkVariable<float> moveSpeed;   //���������ֵ���ͣ������ǽṹ��

    public float moveSpeed = 3;


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
[ServerRpc(RequireOwnership =false)]
    private void HandleMovementServerRpc(Vector3 inputDir)
    {
        //���߷���ˣ����������
        transform.Translate(Time.deltaTime * moveSpeed * inputDir);
    }


}
