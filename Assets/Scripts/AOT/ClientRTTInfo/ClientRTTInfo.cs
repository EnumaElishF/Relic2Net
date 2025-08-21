using JKFrame;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 计算并维护平均RTT：滑动窗口机制，持续计算最近一段时间内的平均 RTT，用于延迟补偿
/// </summary>
public class ClientRTTInfo : SingletonMono<ClientRTTInfo>
{
    public int rttMs { get; private set; } //平均 RTT（毫秒）
    private Queue<int> rttTimeQueue; //队列存储最近calFrames个帧的 RTT 数据
    [SerializeField] private int calFrames = 100; //设定calFrames帧（默认的100帧）
    private int totalMs;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        rttTimeQueue = new Queue<int>(calFrames);
    }
    private void OnDisable()
    {
        rttTimeQueue.Clear();
        totalMs = 0;
        rttMs = 0;
    }
    private void FixedUpdate()
    {
        if (NetManager.Instance == null) return;

        if (NetManager.Instance.IsConnectedClient)
        {
            //若队列中数据量达到calFrames（默认 100），
            //则移除最早的一条数据（出队），并从totalMs中减去该值（维持滑动窗口）。
            if (rttTimeQueue.Count >= 100)
            {
                totalMs -= rttTimeQueue.Dequeue();
            }

            int currentRtt = (int)NetManager.Instance.unityTransport.GetCurrentRtt(NetManager.ServerClientId);
            rttTimeQueue.Enqueue(currentRtt);
            totalMs += currentRtt;
            rttMs = totalMs / rttTimeQueue.Count;
        }
    }
}
