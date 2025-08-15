using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public float moveSpeed = 1;
    void Update()
    {
        if (IsOwner)
        {
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
        transform.Translate(Time.deltaTime * moveSpeed * inputDir);
    }
}
