using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    //多个玩家，所以Player没有单例
    public static Player localPlayer { get; private set; }

    //public NetworkVariable<float> moveSpeed;   //网络变量：值类型，或者是结构体
    public float moveSpeed = 3;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
#if !UNITY_SERVER
        if(IsOwner&& IsClient)
        {
            //客户端快速访问到 “自己控制的玩家对象”
            localPlayer = this;
        }
#endif
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

}

//相当于调用服务端上自身的本体
//---也就是说，完成上面的数据判断后，把最终的坐标移动交给了服务器。
//这就是Rpc好用的地方。
//这里的moveSpeed就算本地客户端作弊，修改了数据，但是服务端在使用的时候用的是服务端的moveSpeed，还达到了防作弊效果。
[ServerRpc(RequireOwnership =false)]
    private void HandleMovementServerRpc(Vector3 inputDir)
    {
        //告诉服务端，有这个事情
        transform.Translate(Time.deltaTime * moveSpeed * inputDir);
    }


}
