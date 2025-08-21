using JKFrame;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���㲢ά��ƽ��RTT���������ڻ��ƣ������������һ��ʱ���ڵ�ƽ�� RTT�������ӳٲ���
/// </summary>
public class ClientRTTInfo : SingletonMono<ClientRTTInfo>
{
    public int rttMs { get; private set; } //ƽ�� RTT�����룩
    private Queue<int> rttTimeQueue; //���д洢���calFrames��֡�� RTT ����
    [SerializeField] private int calFrames = 100; //�趨calFrames֡��Ĭ�ϵ�100֡��
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
            //���������������ﵽcalFrames��Ĭ�� 100����
            //���Ƴ������һ�����ݣ����ӣ�������totalMs�м�ȥ��ֵ��ά�ֻ������ڣ���
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
