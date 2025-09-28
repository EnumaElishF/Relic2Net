using JKFrame;
using System;
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
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_ChatMessage, OnClientChatMessage);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_GetBagData, OnClientGetBagData);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_BagUseItem, OnClientBagUseItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_BagSwapItem, OnClientBagSwapItem);
    }

    #region 物品
    /// <summary>
    /// 当客户端请求背包数据
    /// </summary>
    private void OnClientGetBagData(ulong clientID, INetworkSerializable serializable)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            C_S_GetBagData message = (C_S_GetBagData)serializable;

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
    private void OnClientBagUseItem(ulong clientID, INetworkSerializable serializable)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            C_S_BagUseItem message = (C_S_BagUseItem)serializable;

            //TODO 具体的物品使用
            BagData bagData = client.playerData.bagData;
            //背包数据的TryUseItem原本是放服务器这边的，但是出现Common程序集获取内容的错误，应该是Unity打包识别不足够好，
            //最后我们还是把数据获取内容，转移到BagData
            ItemDataBase itemData = bagData.TryUseItem(message.itemIndex);
            ItemType itemType = ItemType.Empty;
            if (itemData != null) itemType = itemData.GetItemType();
            S_C_BagUpdateItem result = new S_C_BagUpdateItem
            {
                itemIndex = message.itemIndex,
                bagDataVersion = bagData.dataVersion,
                newItemData = itemData,
                itemType = itemType,
                usedWeapon = itemType == ItemType.Weapon
            };
            if(result.usedWeapon) //更新角色的实际武器
            {
#if UNITY_SERVER || UNITY_EDITOR
                client.playerController.UpdateWeaponNetVar(itemData.id);
#endif
            }
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem, result, clientID);
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
    /// <summary>
    /// 当客户端互换背包中的物品
    /// </summary>
    private void OnClientBagSwapItem(ulong clientID, INetworkSerializable serializable)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            C_S_BagSwapItem message = (C_S_BagSwapItem)serializable;
            BagData bagData = client.playerData.bagData;
            //交换数据
            bagData.SwapItem(message.itemIndexA, message.itemIndexB);

            ItemDataBase itemAData = bagData.itemList[message.itemIndexA];
            ItemDataBase itemBData = bagData.itemList[message.itemIndexB];
            ItemType itemAType = itemAData == null ? ItemType.Empty : itemAData.GetItemType();
            ItemType itemBType = itemBData == null ? ItemType.Empty : itemBData.GetItemType();

            S_C_BagUpdateItem resultA = new S_C_BagUpdateItem
            {
                itemIndex = message.itemIndexA,
                bagDataVersion = bagData.dataVersion,
                newItemData = itemAData,
                itemType = itemAType,
                usedWeapon = bagData.usedWeaponIndex == message.itemIndexA,
            };
            bagData.AddDataVersion(); // 避免第二条消息因为版本号是一样的被客户端过滤,版本需要+1位
            S_C_BagUpdateItem resultB = new S_C_BagUpdateItem
            {
                itemIndex = message.itemIndexB,
                bagDataVersion = bagData.dataVersion,
                newItemData = itemBData,
                itemType = itemBType,
                usedWeapon = bagData.usedWeaponIndex == message.itemIndexB,
            };
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem, resultA, clientID);
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem, resultB, clientID);

            //涉及到快捷键的修改，也就是A或者B 的快捷键跟着修改
            if(bagData.TryGetShortcutBarIndex(message.itemIndexA,out int shortcutAIndex))
            {
                //涉及到服务器上数据变更，背包的改动每次加个版本号变化，这样做背包的内容
                bagData.AddDataVersion();
                //A使用的快捷键被替换成B
                S_C_ShortcutBarUpdateItem shortcutBarUpdateItem = new S_C_ShortcutBarUpdateItem
                {
                    shortcutBarIndex = shortcutAIndex,
                    bagIndex = message.itemIndexB,
                    bagDataVersion = bagData.dataVersion
                };
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ShortcutBarUpdateItem, shortcutBarUpdateItem, clientID);
            }
            if(bagData.TryGetShortcutBarIndex(message.itemIndexB,out int shortcutBIndex))
            {
                bagData.AddDataVersion();
                bagData.UpdateShortcutBarIndex(shortcutBIndex, message.itemIndexA);
                S_C_ShortcutBarUpdateItem shortcutBarUpdateItem = new S_C_ShortcutBarUpdateItem
                {
                    shortcutBarIndex = shortcutBIndex,
                    bagIndex = message.itemIndexA,
                    bagDataVersion = bagData.dataVersion
                };
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ShortcutBarUpdateItem, shortcutBarUpdateItem, clientID);
            }
            if (shortcutAIndex != -1)
            {
                bagData.UpdateShortcutBarIndex(shortcutAIndex, message.itemIndexB);
            }
            if (shortcutBIndex != -1)
            {
                bagData.UpdateShortcutBarIndex(shortcutBIndex, message.itemIndexA);
            }
        }
    }

}