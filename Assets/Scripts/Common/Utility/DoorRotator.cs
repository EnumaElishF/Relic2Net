using UnityEngine;

/// <summary>
/// 可推开的旋转门（支持左右门区分，且根据服务端/客户端控制碰撞体）
/// </summary>
public class DoorRotator : MonoBehaviour
{
    [Header("门配置")]
    [Tooltip("旋转轴(0:X, 1:Y, 2:Z)")]
    public int rotateAxis = 1; // 默认绕Y轴旋转
    [Tooltip("旋转速度")]
    public float rotateSpeed = 5f;
    [Tooltip("交互检测触发器（建议使用SphereCollider）")]
    public Collider interactTrigger; // 手动指定触发器碰撞体
    public Collider serverCloseCollider; // 在服务端关闭的碰撞体（客户端保持开启）
    [Tooltip("是否为左门（用于区分旋转方向）")]
    public bool isLeftDoor = false; // 标识是否为左门

    private bool isOpen = false;
    private bool isRotating = false;
    private bool isPlayerInRange = false; // 玩家是否在触发范围内
    private Quaternion targetRotation;
    private Quaternion closedRotation;
    private Transform playerInRange; // 范围内的玩家

    private void Start()
    {
        closedRotation = transform.rotation;

        // 自动配置触发器（如果未指定）
        if (interactTrigger == null)
        {
            interactTrigger = GetComponent<Collider>();
        }

        // 确保触发器配置正确
        if (interactTrigger != null)
        {
            interactTrigger.isTrigger = true;
        }

        // 根据运行环境设置碰撞体状态
        SetupCollidersByEnvironment();
    }

    /// <summary>
    /// 根据是服务端还是客户端设置碰撞体状态
    /// !后续，修改为使用网络消息，客户端的门变化，服务端的也要变化，起码模型网格和碰撞体是要变的，让其他玩家也看到。
    /// </summary>
    private void SetupCollidersByEnvironment()
    {
        // 如果存在需要在服务端关闭的碰撞体
        if (serverCloseCollider != null)
        {
#if UNITY_SERVER || UNITY_EDITOR // 服务端环境（包括编辑器中的服务端模式）
            serverCloseCollider.enabled = false; // 服务端关闭碰撞体
#else // 客户端环境
            serverCloseCollider.enabled = true; // 客户端开启碰撞体
#endif
        }
    }

    private void Update()
    {
        // 旋转逻辑
        if (isRotating)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }

        // 检测玩家交互输入（仅当玩家在范围内）
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F) && !isRotating && playerInRange != null)
        {
            ToggleDoor(playerInRange);
        }
    }

    private void ToggleDoor(Transform playerTransform)
    {
        isOpen = !isOpen;
        isRotating = true;

        // 计算旋转方向（基于玩家面向方向和门类型）
        Vector3 playerForward = playerTransform.forward;
        Vector3 doorToPlayer = playerTransform.position - transform.position;
        doorToPlayer.y = 0; // 忽略Y轴影响

        // 计算玩家相对于门的方向与玩家朝向的夹角
        float angle = Vector3.SignedAngle(doorToPlayer.normalized, playerForward, Vector3.up);

        // 确定旋转方向（考虑左右门差异）
        float baseRotateAngle = isOpen ? 90f : -90f;
        // 门的方向取反，向Player面朝方向开
        baseRotateAngle = -baseRotateAngle;
        if (angle < 0) baseRotateAngle = -baseRotateAngle;

        // 如果是左门，反转旋转方向
        float finalRotateAngle = isLeftDoor ? -baseRotateAngle : baseRotateAngle;

        // 设置目标旋转角度
        Vector3 rotationAxis = rotateAxis == 0 ? Vector3.right : (rotateAxis == 1 ? Vector3.up : Vector3.forward);
        targetRotation = isOpen
            ? closedRotation * Quaternion.Euler(rotationAxis * finalRotateAngle)
            : closedRotation;
    }

    // 触发器检测 - 玩家进入范围
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerInRange = other.transform;
        }
    }

    // 触发器检测 - 玩家离开范围
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerInRange = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (interactTrigger != null)
        {
            Gizmos.color = isLeftDoor ? Color.cyan : Color.blue;
            // 绘制触发器范围
            if (interactTrigger is SphereCollider sphere)
            {
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
            else if (interactTrigger is BoxCollider box)
            {
                Gizmos.DrawWireCube(transform.position + box.center, box.size);
            }
        }
    }
}
