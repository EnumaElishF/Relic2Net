using JKFrame;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 负责聊天系统的部分
/// </summary>
public partial class ClientsManager : SingletonMono<ClientsManager>
{
    /// <summary>
    /// 虽然是一个文件，都是ClientsManager，但是我们还是按照系统的思路来
    /// </summary>
    public void InitChatSystem()
    {
        //NetMessageManager注册网络事件
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.C_S_ChatMessage, OnClientChatMessage);
    }

    #region 聊天
    /// <summary>
    /// 当客户端发来聊天消息。服务器对客户端的聊天消息发送: 广播给全部的游戏中玩家
    /// </summary>
    private void OnClientChatMessage(ulong clientID, INetworkSerializable serializable)
    {
        string chatMessage = ((C_S_ChatMessage)serializable).message;
        if (string.IsNullOrWhiteSpace(chatMessage)) return; //消息有效性验证
        if (!clientIDDic.TryGetValue(clientID, out Client sourceClient) && sourceClient.playerData == null) return;//检查源头客户端的有效性
        //发送给所有游戏状态下的客户端
        if (clientStateDic.TryGetValue(ClientState.Gaming, out HashSet<Client> clients))
        {
            S_C_ChatMessage message = new S_C_ChatMessage { playerName = sourceClient.playerData.name, message = chatMessage };
            foreach (Client client in clients)
            {
                //这样就把消息转发出去了
                NetMessageManager.Instance.SendMessageToClient(MessageType.S_C_ChatMessage, message, client.clientID);
            }
        }

    }
    #endregion

}