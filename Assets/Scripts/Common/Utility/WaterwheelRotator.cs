using UnityEngine;

public class WaterwheelRotator : MonoBehaviour
{
    // 每秒旋转的角度（X轴）
    [SerializeField] private float degreesPerSecond = 5f;

    // 每帧旋转的角度
    private float degreesPerFrame;

    void Start()
    {
        // 计算每帧应该旋转的角度
        // 基于30帧/秒的设定，每帧旋转角度 = 每秒旋转角度 / 帧率
        degreesPerFrame = degreesPerSecond / 30f;
    }

    void Update()
    {
        // 绕X轴旋转
        transform.Rotate(degreesPerFrame, 0, 0);
    }
}