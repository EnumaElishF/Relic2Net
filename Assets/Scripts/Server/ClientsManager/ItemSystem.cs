using JKFrame;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 负责物品系统的部分
/// </summary>
public partial class ClientsManager : SingletonMono<ClientsManager>
{
    /// <summary>
    /// 虽然是一个文件，都是ClientsManager，但是我们还是按照系统的思路来
    /// </summary>
    public void InitItemSystem()
    {
        PlayerController.SetGetWeaponFunc(GetWeapon);

        //NetMessageManager注册网络事件
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_GetBagData, OnClientGetBagData);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_UseItem, OnClientUseItem);
    }

    #region 物品
    /// <summary>
    /// 当客户端请求背包数据
    /// </summary>
    private void OnClientGetBagData(ulong clientID, INetworkSerializable serializable)
    {
        C_S_GetBagData message = (C_S_GetBagData)serializable;
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            S_C_GetBagData result = new S_C_GetBagData
            {
                haveBagData = client.playerData.bagData.dataVersion != message.dataVersion
            };
            //客户端这边新版背包数据不为空，且客户端的背包版本发生变化，才会执行
            if (result.haveBagData) result.bagData = client.playerData.bagData;
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_GetBagData, result, clientID);
        }
    }
    /// <summary>
    /// 当客户端请求使用物品
    /// </summary>
    private void OnClientUseItem(ulong clientID, INetworkSerializable serializable)
    {
        C_S_UseItem message = (C_S_UseItem)serializable;
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            //TODO 具体的物品使用
            BagData bagData = client.playerData.bagData;
            //背包数据的TryUseItem原本是放服务器这边的，但是出现Common程序集获取内容的错误，应该是Unity打包识别不足够好，
            //最后我们还是把数据获取内容，转移到BagData
            ItemDataBase itemData = bagData.TryUseItem(message.itemIndex);
            ItemType itemType = ItemType.Empty;
            if (itemData != null) itemType = itemData.GetItemType();
            S_C_UpdateItem result = new S_C_UpdateItem
            {
                itemIndex = message.itemIndex,
                bagDataVersion = bagData.dataVersion,
                newItemData = itemData,
                itemType = itemType
            };
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_UpdateItem, result, clientID);
        }

    }
    #endregion

    private GameObject GetWeapon(string weaponName)
    {
        GameObject weaponObj = PoolSystem.GetGameObject(weaponName);
        Debug.Log("weaponName是:" + weaponName);
        if (weaponObj == null)
        {
            WeaponConfig weaponConfig = ServerResSystem.GetItemConfig<WeaponConfig>(weaponName);
            if(weaponConfig == null) Debug.Log("weaponConfig获取失败,weaponName是:" + weaponName);
            weaponObj = Instantiate(weaponConfig.prefab);
            weaponConfig.name = weaponName;
        }
        return weaponObj;
    }
}