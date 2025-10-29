using JKFrame;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 公共   : 公共的地方大部分都是做分支的，分成客户端，和 服务端  
/// PlayerController做mainController在Common程序集下，去采用PlayerServerController的部分数值
/// </summary>
/// 
public partial class PlayerController : NetworkBehaviour
{
    #region 静态成员 static
    private static Func<string, GameObject> getWeaponFunc;
    public static void SetGetWeaponFunc(Func<string,GameObject> func)
    {
        getWeaponFunc = func;
        Debug.Log($"委托绑定：{func.Method.DeclaringType.Name}.{func.Method.Name}"); // 输出绑定的方法所在类
    }
    #endregion
    #region  面板赋值 (理论上，下面这些值，包括移动速度旋转速度等，客户端都不需要知道，只要服务端知道就行) 服务端基于根运动移动

    public Player_View view { get; private set; }
    #endregion
    public NetVariable<PlayerState> currentState = new NetVariable<PlayerState>(PlayerState.None);
    //新程序集，要加入对Common的新程序集的引用 Unity.Collections;
    public NetVariable<FixedString32Bytes> usedWeaponName = new NetVariable<FixedString32Bytes>();
    public NetVariable<FixedString32Bytes> playerName = new NetVariable<FixedString32Bytes>();

    //多个玩家，所以Player没有单例
    //public NetworkVariable<float> moveSpeed;   //网络变量：值类型，或者是结构体

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        usedWeaponName.OnValueChanged = OnUsedWeaponNameChanged;
        if (view == null) // 意味着没有初始化过
        {
            //直接查玩家下的游戏对象Player_kazuma的脚本Player_View
            view = transform.Find("Player_kazuma").GetComponent<Player_View>();
        }
        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            Client_OnNetworkSpawn();
#endif
        }
        else
        {
#if UNITY_SERVER || UNITY_EDITOR
            Server_OnNetworkSpawn();
#endif
        }
        UpdateWeaponObject(usedWeaponName.Value.ToString());
    }

    private void OnUsedWeaponNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        UpdateWeaponObject(newValue.ToString());
    }
    private void UpdateWeaponObject(string weaponID)
    {
        if (string.IsNullOrWhiteSpace(weaponID)) return; //weapon上关于playerData的数据还没执行获得到呢的话，就不先执行下面的内容
        GameObject weaponGameObject = getWeaponFunc.Invoke(weaponID);
        view.SetWeapon(weaponGameObject);
    }

    /// <summary>
    /// 玩家下线，Despawn消除玩家
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            
#endif
        }
        else
        {
#if UNITY_SERVER || UNITY_EDITOR
            Server_OnNetworkDespawn();
#endif
        }
    }

    //相当于调用 "服务端" 上自身的本体
    //---也就是说，完成上面的数据判断后，把最终的坐标移动交给了服务器。
    //这就是Rpc好用的地方。
    //这里的moveSpeed就算本地客户端作弊，修改了数据，但是服务端在使用的时候用的是服务端的moveSpeed，还达到了防作弊效果。
    //???是true还是false
    [ServerRpc(RequireOwnership = true)]  //只会被服务端调用的方法,RequireOwnership验证是否宿主
    public void SendInputMoveDirServerRpc(Vector3 moveDir)
    {
#if UNITY_SERVER || UNITY_EDITOR
        Server_ReceiveMoveInput(moveDir);
#endif
    }
    [ServerRpc(RequireOwnership = true)]
    public void SendJumpInputServerRpc()
    {
#if UNITY_SERVER || UNITY_EDITOR
        Server_ReceiveJumpInput();
#endif
    }
}

/// <summary>
/// 客户端
/// </summary>
#if !UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController : NetworkBehaviour
{
    private void Start()
    {
        // Start 一定 在OnNetworkSpawn后执行，如果这个阶段IsSpawned = false 说明是个异常对象
        if (!IsSpawned)
        {
            //网络对象对象池
            //玩家如果退出登录,用这个可以销毁，但是我们有AOI相关的东西，不能仅仅把游戏对象销毁就结束。 
            NetManager.Instance.DestroyObject(this.NetworkObject);
        }
    }
    private void Client_OnNetworkSpawn()
    {
        EventSystem.TypeEventTrigger(new SpawnPlayerEvent { newPlayer = this });
    }

}
#endif


/// <summary>
/// 服务端
/// </summary>
#if UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController : NetworkBehaviour,IStateMachineOwner
{
    private IPlayerServerController serverController;
    public void SetServerController (IPlayerServerController serverController)
    {
        this.serverController = serverController;
    }

    private void Server_OnNetworkSpawn()
    {
        serverController.OnNetworkSpawn();

    }
    /// <summary>
    /// 服务端 把输入保存起来
    /// </summary>
    private void Server_ReceiveMoveInput(Vector3 moveDir)
    {
        serverController.ReceiveMoveInput(moveDir);
    }
    private void Server_ReceiveJumpInput()
    {
        serverController.ReceiveJumpInput();
    }
    /// <summary>
    /// 服务端玩家下线
    /// </summary>
    private void Server_OnNetworkDespawn()
    {
        serverController.OnNetworkDespawn();

    }

 
    /// <summary>
    /// 更新武器网络变量
    /// </summary>
    /// <param name="weaponID"></param>
    public void UpdateWeaponNetVar(string weaponID)
    {
        usedWeaponName.Value = weaponID;
    }
}
#endif
