using JKFrame;
using System;
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

        //TODO C到S端的，NetMessageManager注册网络事件： 服务端控制背包数据变化的关键
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_GetBagData, OnClientGetBagData);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_BagUseItem, OnClientBagUseItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_BagSwapItem, OnClientBagSwapItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_ShortcutBarSetItem, OnClientShortcutBarSetItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_ShortcutBarSwapItem, OnClientShortcutBarSwapItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_ShopBuyItem, OnClientShopBuyItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_BagSellItem, OnClientBagSellItem);
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_CraftItem, OnClientCraftItem);
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
            //具体的物品使用
            BagData bagData = client.playerData.bagData;

            //背包数据的TryUseItem原本是放服务器这边的，但是出现Common程序集获取内容的错误，应该是Unity打包识别不足够好，
            //最后我们还是把数据获取内容，转移到BagData
            if (!bagData.CheckBagIndexRange(message.bagIndex)) return;
            ItemDataBase itemData = bagData.TryUseItem(message.bagIndex);
            ItemType itemType = GlobalUtility.GetItemType(itemData);
            S_C_BagUpdateItem result = new S_C_BagUpdateItem
            {
                itemIndex = message.bagIndex,
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
            if (!bagData.CheckBagIndexRange(message.bagIndexA) || !bagData.CheckBagIndexRange(message.bagIndexB)) return;
            //交换数据
            bagData.SwapItem(message.bagIndexA, message.bagIndexB);

            ItemDataBase itemAData = bagData.itemList[message.bagIndexA];
            ItemDataBase itemBData = bagData.itemList[message.bagIndexB];
            ItemType itemAType = itemAData == null ? ItemType.Empty : itemAData.GetItemType();
            ItemType itemBType = itemBData == null ? ItemType.Empty : itemBData.GetItemType();

            S_C_BagUpdateItem resultA = new S_C_BagUpdateItem
            {
                itemIndex = message.bagIndexA,
                bagDataVersion = bagData.dataVersion,
                newItemData = itemAData,
                itemType = itemAType,
                usedWeapon = bagData.usedWeaponIndex == message.bagIndexA,
            };
            bagData.AddDataVersion(); // 避免第二条消息因为版本号是一样的被客户端过滤,版本需要+1位
            S_C_BagUpdateItem resultB = new S_C_BagUpdateItem
            {
                itemIndex = message.bagIndexB,
                bagDataVersion = bagData.dataVersion,
                newItemData = itemBData,
                itemType = itemBType,
                usedWeapon = bagData.usedWeaponIndex == message.bagIndexB,
            };
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem, resultA, clientID);
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem, resultB, clientID);

            //涉及到快捷键的修改，也就是A或者B 的快捷键跟着修改
            if(bagData.TryGetShortcutBarIndex(message.bagIndexA,out int shortcutAIndex))
            {
                //涉及到服务器上数据变更，背包的改动每次加个版本号变化，这样做背包的内容
                bagData.AddDataVersion();
                //A使用的快捷键被替换成B
                S_C_ShortcutBarUpdateItem shortcutBarUpdateItem = new S_C_ShortcutBarUpdateItem
                {
                    shortcutBarIndex = shortcutAIndex,
                    bagIndex = message.bagIndexB,
                    bagDataVersion = bagData.dataVersion
                };
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ShortcutBarUpdateItem, shortcutBarUpdateItem, clientID);
            }
            if(bagData.TryGetShortcutBarIndex(message.bagIndexB,out int shortcutBIndex))
            {
                bagData.AddDataVersion();
                bagData.UpdateShortcutBarItem(shortcutBIndex, message.bagIndexA);
                S_C_ShortcutBarUpdateItem shortcutBarUpdateItem = new S_C_ShortcutBarUpdateItem
                {
                    shortcutBarIndex = shortcutBIndex,
                    bagIndex = message.bagIndexA,
                    bagDataVersion = bagData.dataVersion
                };
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ShortcutBarUpdateItem, shortcutBarUpdateItem, clientID);
            }
            if (shortcutAIndex != -1)
            {
                bagData.UpdateShortcutBarItem(shortcutAIndex, message.bagIndexB);
            }
            if (shortcutBIndex != -1)
            {
                bagData.UpdateShortcutBarItem(shortcutBIndex, message.bagIndexA);
            }
        }
    }

    /// <summary>
    /// 当客户端设置物品快捷键
    /// </summary>
    private void OnClientShortcutBarSetItem(ulong clientID, INetworkSerializable serializable)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            C_S_ShortcutBarSetItem message = (C_S_ShortcutBarSetItem)serializable;
            BagData bagData = client.playerData.bagData;

            message.bagIndex = bagData.CheckBagIndexRange(message.bagIndex) ? message.bagIndex : -1;
            if (!bagData.CheckShortcutBarIndexRange(message.shortcutBarIndex)) return;

            // 找到当前快捷栏中可能存在的重复项，将其移除
            if (bagData.TryGetShortcutBarIndex(message.bagIndex,out int shortcutBarIndex))
            {
                bagData.RemoveShortcutBarItem(shortcutBarIndex);
                bagData.AddDataVersion();
                //更新的是删除
                S_C_ShortcutBarUpdateItem result1 = new S_C_ShortcutBarUpdateItem
                {
                    shortcutBarIndex = shortcutBarIndex,
                    bagIndex = -1,
                    bagDataVersion = bagData.dataVersion
                };
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ShortcutBarUpdateItem, result1, clientID);
            }
            //设置修改后的快捷键
            //其实就是换了一条消息
            bagData.UpdateShortcutBarItem(message.shortcutBarIndex, message.bagIndex);
            bagData.AddDataVersion();
            //更新的是修改
            S_C_ShortcutBarUpdateItem result2 = new S_C_ShortcutBarUpdateItem
            {
                shortcutBarIndex = message.shortcutBarIndex,
                bagIndex = message.bagIndex,
                bagDataVersion = bagData.dataVersion
            };
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ShortcutBarUpdateItem, result2, clientID);

        }
    }
    /// <summary>
    /// 当客户端交换快捷键
    /// </summary>
    private void OnClientShortcutBarSwapItem(ulong clientID, INetworkSerializable serializable)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            C_S_ShortcutBarSwapItem message = (C_S_ShortcutBarSwapItem)serializable;
            BagData bagData = client.playerData.bagData;
            if (!bagData.CheckShortcutBarIndexRange(message.shortcutBarIndexA) || !bagData.CheckShortcutBarIndexRange(message.shortcutBarIndexB)) return;
            bagData.SwapShortcutBarItem(message.shortcutBarIndexA, message.shortcutBarIndexB);
            //消息1
            bagData.AddDataVersion();
            S_C_ShortcutBarUpdateItem result1 = new S_C_ShortcutBarUpdateItem
            {
                shortcutBarIndex = message.shortcutBarIndexA,
                bagIndex = bagData.shortcutBarIndexs[message.shortcutBarIndexA],
                bagDataVersion = bagData.dataVersion
            };
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ShortcutBarUpdateItem, result1, clientID);

            //消息2
            bagData.AddDataVersion();
            S_C_ShortcutBarUpdateItem result2 = new S_C_ShortcutBarUpdateItem
            {
                shortcutBarIndex = message.shortcutBarIndexB,
                bagIndex = bagData.shortcutBarIndexs[message.shortcutBarIndexB],
                bagDataVersion = bagData.dataVersion
            };
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ShortcutBarUpdateItem, result2, clientID);
        }
    }
    /// <summary>
    /// 当客户端从商店购买物品
    /// </summary>
    private void OnClientShopBuyItem(ulong clientID, INetworkSerializable serializable)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            C_S_ShopBuyItem message = (C_S_ShopBuyItem)serializable;
            BagData bagData = client.playerData.bagData;
            if (!bagData.CheckBagIndexRange(message.bagIndex)) return;
            // 物品不存在的判断
            ItemConfigBase itemConfig = ServerResSystem.GetItemConfig<ItemConfigBase>(message.itemID);
            if (itemConfig == null) return;
            // 金币检查: 钱不够就直接return掉
            if(bagData.coinCount < itemConfig.price) return;
            if (bagData.TryAddItem(itemConfig, 1, message.bagIndex))
            {
                bagData.AddDataVersion();
                //回复客户端增加物品
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem,
                    new S_C_BagUpdateItem
                    {
                        itemIndex = message.bagIndex,
                        bagDataVersion = bagData.dataVersion,
                        newItemData = bagData.itemList[message.bagIndex],
                        itemType = GlobalUtility.GetItemType(bagData.itemList[message.bagIndex]),
                        usedWeapon = false
                    }, clientID);
                //回复客户端金币更新
                bagData.coinCount -= itemConfig.price;
                bagData.AddDataVersion();
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_UpdateCoinCount,
                    new S_C_UpdateCoinCount
                    {
                        bagDataVersion = bagData.dataVersion,
                        coinCount = bagData.coinCount,
                    }, clientID);
            }

        }
    }
    /// <summary>
    /// 当客户端从背包出售物品
    /// </summary>
    public void OnClientBagSellItem(ulong clientID, INetworkSerializable serializable)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            C_S_BagSellItem message = (C_S_BagSellItem)serializable;
            BagData bagData = client.playerData.bagData;
            //条件：index范围有效，物品不是空格子，不是当前使用的武器
            if (!bagData.CheckBagIndexRange(message.bagIndex) || message.bagIndex == bagData.usedWeaponIndex) return;
            ItemDataBase itemData = bagData.itemList[message.bagIndex];
            if (itemData == null) return;

            ItemConfigBase itemConfig = ServerResSystem.GetItemConfig<ItemConfigBase>(bagData.itemList[message.bagIndex].id);
            //销毁物品
            bagData.RemoveItem(message.bagIndex);
            bagData.AddDataVersion();
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem,
                new S_C_BagUpdateItem
                {
                    itemIndex = message.bagIndex,
                    bagDataVersion = bagData.dataVersion,
                    newItemData = null,
                    itemType = ItemType.Empty,
                    usedWeapon = false
                }, clientID);
            int itemCount = 1;
            if (itemData is StackableItemDataBase)
            {
                itemCount = ((StackableItemDataBase)itemData).count;
            }
            //增加金币: 售出价格为原价格的一半
            bagData.coinCount += (itemConfig.price / 2) * itemCount;
            bagData.AddDataVersion();
            NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_UpdateCoinCount,
                new S_C_UpdateCoinCount
                {
                    bagDataVersion = bagData.dataVersion,
                    coinCount = bagData.coinCount,
                }, clientID);
        }
    }
    /// <summary>
    /// 当客户端合成物品
    /// </summary>
    private void OnClientCraftItem(ulong clientID, INetworkSerializable serializable)
    {
        if (clientIDDic.TryGetValue(clientID, out Client client) && client.playerData != null)
        {
            C_S_CraftItem message = (C_S_CraftItem)serializable;
            //要合成的目标物品
            ItemConfigBase targetItemConfig = ServerResSystem.GetItemConfig<ItemConfigBase>(message.targetItemName);
            if (targetItemConfig == null) return;
            BagData bagData = client.playerData.bagData;
            if(bagData.CheckCraft(targetItemConfig,out bool containUsedWeapon))
            {
                int updateItemIndex = -1; // 最终更新物品的位置
                if (containUsedWeapon) //合成中需要的材料涉及到当前使用武器的，必须目标物品也是武器，进行替换
                {
                    if(targetItemConfig is WeaponConfig)
                    {
                        updateItemIndex = bagData.usedWeaponIndex;
                        //覆盖掉之前的武器
                        bagData.itemList[updateItemIndex] = targetItemConfig.GetDefaultItemData().Copy();
                        client.playerController.UpdateWeaponNetVar(targetItemConfig.name);
                    }
                }else if (bagData.TryAddItem(targetItemConfig,1,out updateItemIndex)) //尝试添加
                {
                    // 移除全部用来合成的物品
                    foreach(System.Collections.Generic.KeyValuePair<string, int> item in targetItemConfig.craftConfig.itemDic)
                    {
                        ItemDataBase itemData = bagData.TryGetItem(item.Key, out int itemIndex);
                        //如果涉及到当前武器的，之前已经处理过了
                        if (containUsedWeapon && itemIndex == bagData.usedWeaponIndex) continue;
                        bagData.RemoveItem(itemIndex, item.Value);
                        bagData.AddDataVersion();
                        NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem, new S_C_BagUpdateItem
                        {
                            itemIndex = itemIndex,
                            bagDataVersion = bagData.dataVersion,
                            newItemData = bagData.itemList[itemIndex],
                            itemType = GlobalUtility.GetItemType(bagData.itemList[itemIndex]),
                            usedWeapon = false
                        }, clientID);
                    }
                }
                if(updateItemIndex != -1)
                {
                    bagData.AddDataVersion();
                    NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_BagUpdateItem, new S_C_BagUpdateItem
                    {
                        itemIndex = updateItemIndex,
                        bagDataVersion = bagData.dataVersion,
                        newItemData = bagData.itemList[updateItemIndex],
                        itemType = GlobalUtility.GetItemType(bagData.itemList[updateItemIndex]),
                        usedWeapon = containUsedWeapon
                    }, clientID);
                }
            }
        }

    }


}