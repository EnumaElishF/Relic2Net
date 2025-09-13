using JKFrame;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ChatWindow : UI_CustomWindowBase
{
    private const int itemCount = 15;
    [SerializeField] private Transform main;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform itemRoot;
    [SerializeField] private InputField chatInputField;
    private Image scrollRectImage;
    //itemCount条消息满后，有新消息会把旧的顶出队列
    private Queue<UI_ChatWindowItem> itemQueue = new Queue<UI_ChatWindowItem>(itemCount);

    public override void Init()
    {
        scrollRectImage = scrollRect.GetComponent<Image>();
        //回车输入框
        chatInputField.onSubmit.AddListener(OnChatInputFieldSubmit);
        //改变聊天窗口的Main里组件有图片，控制透明色
        main.OnMouseEnter(OnEnter);
        main.OnMouseExit(OnExit);
        //默认退出聊天框
        OnExit(null);
    }
    public override void OnShow()
    {
        base.OnShow();
        //服务端接收到消息后，把消息发回来
        NetMessageManager.Instance.RegisterMessageCallback(MessageType.S_C_ChatMessage, OnChatMessage);
    }
    /// <summary>
    /// 服务端发给玩家的信息serializable
    /// </summary>
    private void OnChatMessage(ulong serverID, INetworkSerializable serializable)
    {
        S_C_ChatMessage message = (S_C_ChatMessage)serializable;
        AddItem(message.playerName, message.message);
    }
    /// <summary>
    /// 做到鼠标进入强化显示
    /// </summary>
    private void OnEnter(PointerEventData data)
    {
        chatInputField.gameObject.SetActive(true);
        scrollRect.vertical = true;
        //背景图透明度修改
        scrollRectImage.color = new Color(0, 0, 0, 0.2f);
    }
    /// <summary>
    /// 鼠标离开弱化显示
    /// </summary>
    private void OnExit(PointerEventData data)
    {
        chatInputField.gameObject.SetActive(false);
        scrollRect.vertical = false;
        scrollRectImage.color = new Color(0, 0, 0, 0);
    }

    private void OnChatInputFieldSubmit(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return; // 避免发送纯空格
        // 发送网络消息给服务端，发送聊天消息
        NetMessageManager.Instance.SendMessageToServer(MessageType.C_S_ChatMessage, new C_S_ChatMessage
        {
            message = content
        });
        chatInputField.text = "";
        chatInputField.Select();
        chatInputField.ActivateInputField();    // 让输入框重新成为焦点
    }

    public void AddItem(string name, string content)
    {
        // 滚动条：如果原本就在最下方，在收到新消息时，要自动滑到最下方。scrollRect的垂直方向数值
        bool onEnd = scrollRect.verticalNormalizedPosition <= 0.1f; // 0代表最下方
        // 清除最上方的：也就是出队列
        if (itemQueue.Count >= itemCount)
        {
            DestroyItem(itemQueue.Dequeue());
        }
        UI_ChatWindowItem item = CreateItem();
        itemQueue.Enqueue(item);
        item.Init(name, content);
        // 有一种特殊情况，就是在未填满之前（消息不够多）verticalNormalizedPosition会一直是0，但是当超出范围的瞬间会变成1，这种情况也要去底部
        // Unity的更新构建机制并不是当前帧(刚改完的一帧是没有直接生效的)，所以最好延迟2帧去执行操作
        // UI 布局（包括 ScrollRect 的内容大小、滚动范围等）更新属于延迟计算，在后续的布局重建（Layout Rebuild） 阶段才重新计算
        if (onEnd)
        {
            StartCoroutine(LockScorllToEnd());
        }
    }

    private IEnumerator LockScorllToEnd()
    {
        yield return CoroutineTool.WaitForFrames(2);
        scrollRect.verticalNormalizedPosition = 0;
    }
    /// <summary>
    /// 创建
    /// </summary>
    /// <returns></returns>
    private UI_ChatWindowItem CreateItem()
    {
        //使用ResSystem实例化，同时设计上也能自动帮忙考虑对象池的问题
        return ResSystem.InstantiateGameObject<UI_ChatWindowItem>(nameof(UI_ChatWindowItem), itemRoot);
    }
    /// <summary>
    /// 销毁:直接放入对象池，方便再次使用
    /// </summary>
    private void DestroyItem(UI_ChatWindowItem item)
    {
        item.GameObjectPushPool();
    }
    /// <summary>
    /// 窗口关闭，不再回到界面，直接Clear掉
    /// </summary>
    public override void OnClose()
    {
        base.OnClose();
        //取消注册，服务端对玩家发送的消息反馈
        NetMessageManager.Instance.UnRegisterMessageCallback(MessageType.S_C_ChatMessage, OnChatMessage);
        PoolSystem.ClearGameObject(nameof(UI_ChatWindowItem));
    }
}
