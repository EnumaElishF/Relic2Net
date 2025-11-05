using JKFrame;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


/// <summary>
/// 公共   : 公共的地方大部分都是做分支的，分成客户端，和 服务端  
/// PlayerController做mainController在Common程序集下，去采用PlayerServerController的部分数值
/// </summary>
public partial class PlayerController : CharacterControllerBase<PlayerView,IPlayerClientController,IPlayerServerController>
{
    #region 静态成员 static
    private static Func<string, GameObject> getWeaponFunc;
    public static void SetGetWeaponFunc(Func<string, GameObject> func)
    {
        getWeaponFunc = func;
        Debug.Log($"委托绑定：{func.Method.DeclaringType.Name}.{func.Method.Name}"); // 输出绑定的方法所在类
    }
    #endregion

    //TODO: 下面按照使用到的，去定义网络变量
    public NetVariable<PlayerState> currentState = new NetVariable<PlayerState>(PlayerState.None);
    public NetVariable<FixedString32Bytes> usedWeaponName = new NetVariable<FixedString32Bytes>();
    public NetVariable<FixedString32Bytes> playerName = new NetVariable<FixedString32Bytes>();

    //FixedString32Bytes作为对中文的值类型做存储，UTF-8 属于新的程序集，要加入对Common的新程序集的引用 Unity.Collections;
    //并且FixedString32Bytes作对比String有很多优势：序列化效率更高，gc开销低，跨平台性好。FixedString32Bytes是值类型，String引用类型。
    //多个玩家，所以Player没有单例
    //public NetworkVariable<float> moveSpeed;   //网络变量：值类型，或者是结构体
    public override void OnNetworkSpawn()
    {
#if !UNITY_SERVER || UNITY_EDITOR
        if (IsClient)
        {
            //初始化生成PlayerController，要优先于clientController调用之前
            EventSystem.TypeEventTrigger(new SpawnPlayerEvent { newPlayer = this }); 
        }
#endif
        base.OnNetworkSpawn();
        usedWeaponName.OnValueChanged = OnUsedWeaponNameChanged;
        UpdateWeaponObject(usedWeaponName.Value.ToString());
    }

    private void OnUsedWeaponNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        UpdateWeaponObject(newValue.ToString());
    }
    public event Action<GameObject> onUpdateWeaponObjectAction;
    /// <summary>
    /// 变更武器
    /// </summary>
    private void UpdateWeaponObject(string weaponID)
    {
        if (string.IsNullOrWhiteSpace(weaponID)) return; //weapon上关于playerData的数据还没执行获得到呢的话，就不先执行下面的内容
        GameObject weaponGameObject = getWeaponFunc.Invoke(weaponID);
        view.SetWeapon(weaponGameObject);
        //当变更武器的时候就回调一下这个event
        onUpdateWeaponObjectAction?.Invoke(weaponGameObject);
    }

    #region ServerRPC
    //相当于调用 "服务端" 上自身的本体
    //---也就是说，完成上面的数据判断后，把最终的坐标移动交给了服务器。
    //这就是Rpc好用的地方。
    //这里的moveSpeed就算本地客户端作弊，修改了数据，但是服务端在使用的时候用的是服务端的moveSpeed，还达到了防作弊效果。
    //???是true还是false
    [ServerRpc(RequireOwnership = true)]  //只会被服务端调用的方法,RequireOwnership验证是否宿主
    public void SendInputMoveDirServerRpc(Vector3 moveDir)
    {
#if UNITY_SERVER || UNITY_EDITOR
        serverController.ReceiveMoveInput(moveDir);
#endif
    }
    [ServerRpc(RequireOwnership = true)]
    public void SendJumpInputServerRpc()
    {
#if UNITY_SERVER || UNITY_EDITOR
        serverController.ReceiveJumpInput();
#endif
    }
    [ServerRpc(RequireOwnership = true)]
    public void SendAttackInputServerRpc(bool value)
    {
#if UNITY_SERVER || UNITY_EDITOR
        serverController.ReceiveAttackInput(value);
#endif
    }

    #endregion

}


/// <summary>
/// 客户端
/// </summary>
#if !UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController
{

}
#endif


/// <summary>
/// 服务端
/// </summary>
#if UNITY_SERVER || UNITY_EDITOR
public partial class PlayerController : IStateMachineOwner
{
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

