using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
//公共 
public abstract partial class CharacterControllerBase<V,C,S> : NetworkBehaviour where V : CharacterViewBase where C : ICharacterClientController where S : ICharacterServerController
{
    [SerializeField] protected V view; //Player_View,我们做泛型定义为V吧, 把View通过面板去序列化吧
    public V View { get => view; }

    public List<SkillConfig> skillConfigList = new List<SkillConfig>();
    public NetVariable<float> currentHp = new NetVariable<float>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            clientController.OnNetworkSpawn();
#endif
        }
        else
        {
#if UNITY_SERVER || UNITY_EDITOR
            if (serverController == null)
            {
                Debug.Log("serverController空");
            }
            serverController.OnNetworkSpawn();
#endif
        }
    }

    /// <summary>
    /// 玩家下线，Despawn消除玩家，从网络上消除
    /// </summary>
    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
#if !UNITY_SERVER || UNITY_EDITOR
            clientController.OnNetworkDespawn();
#endif
        }
        else
        {
#if UNITY_SERVER || UNITY_EDITOR
            serverController.OnNetworkDespawn();
#endif
        }
    }

    #region ClientRPC
    //Client的PRC其实就是去完成接口IPlayerClientController里的几个函数
    [ClientRpc]
    public void StartSkillClientRpc(int skillIndex)
    {
#if !UNITY_SERVER || UNITY_EDITOR
        clientController.StartSkill(skillIndex);
#endif
    }
    [ClientRpc]
    public void StartSkillHitClientRpc()
    {
#if !UNITY_SERVER || UNITY_EDITOR
        clientController.StartSkillHit();
#endif
    }
    [ClientRpc]
    public void PlaySkillHitEffectClientRpc(Vector3 point)//特效命中点
    {
#if !UNITY_SERVER || UNITY_EDITOR
        clientController.PlaySkillHitEffect(point);
#endif
    }
    #endregion

}
/// <summary>
/// 客户端
/// </summary>
#if !UNITY_SERVER || UNITY_EDITOR
public abstract partial class CharacterControllerBase<V, C, S>
{
    //C 继承 IPlayerClientController接口
    protected C clientController;
    public void SetClientController(C clientController)
    {
        this.clientController = clientController;
    }
}
#endif

/// <summary>
/// 服务端
/// </summary>
#if UNITY_SERVER || UNITY_EDITOR
public abstract partial class CharacterControllerBase<V, C, S>
{
    //用protect的吧，毕竟他的子类需要去访问这个参数，所以这样做
    protected S serverController;
    public void SetServerController(S serverController)
    {
        this.serverController = serverController;
    }
}
#endif