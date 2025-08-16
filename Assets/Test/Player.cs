using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<float> moveSpeed;   //网络变量：值类型，或者是结构体
#if UNITY_EDITOR
    public bool test;
#endif

    public override void OnNetworkSpawn()
    {
        moveSpeed = new NetworkVariable<float>(1);
    }
    void Update()
    {
        if (!IsSpawned) return; //如果还未产卵，就不执行下面逻辑

        if (IsOwner)
        {
            //IsOwner 是一个布尔值，用于判断当前本地客户端是否是该 Network Object 的 “所有者”（Owner）。
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(h, 0, v).normalized;
            HandleMovementServerRpc(inputDir);
        }
        if (IsServer)
        {
            //IsServer 是一个布尔值，用于判断当前运行的程序实例是否是服务器（包括 “宿主（Host）”—— 因为 Host 同时扮演服务器和客户端的角色）。
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

//相当于调用服务端上自身的本体
//---也就是说，完成上面的数据判断后，把最终的坐标移动交给了服务器。
//这就是Rpc好用的地方。
//这里的moveSpeed就算本地客户端作弊，修改了数据，但是服务端在使用的时候用的是服务端的moveSpeed，还达到了防作弊效果。
[ServerRpc(RequireOwnership =false)]
    private void HandleMovementServerRpc(Vector3 inputDir)
    {
        //告诉服务端，有这个事情
        transform.Translate(Time.deltaTime * moveSpeed.Value * inputDir);
    }

    [ClientRpc]
    private void TestClientRpc()
    {
        //做一个实例化游戏物品的测试
        GameObject.CreatePrimitive(PrimitiveType.Capsule).transform.position  =transform.position;
    }
}
