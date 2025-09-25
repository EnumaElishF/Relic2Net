using JKFrame;
using System;
using UnityEngine;

public class Player_View : MonoBehaviour
{
    [SerializeField] private Transform weaponRoot;
    private GameObject currentWeapon;
    public void SetWeapon(GameObject weapon)
    {
        if (currentWeapon != null) currentWeapon.GameObjectPushPool();
        if (weapon != null)
        {
            Debug.Log("武器不为空");
            weapon.transform.parent = weaponRoot;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localEulerAngles = Vector3.zero;
        }
        currentWeapon = weapon;
    }

    /// <summary>
    /// Player的动画根运动，RootMotion是需要传递给服务端的(比较重要),客户端倒是不需要
    /// ！网络同步场景，比如服务器需要基于根运动计算角色权威位置，再同步给客户端
    /// </summary>
#if UNITY_SERVER || UNITY_EDITOR
    [SerializeField] private Animator animator;
    private Action<Vector3, Quaternion> rootMotionAction; //委托（回调函数）,传递根运动给外部
    private void OnAnimatorMove()
    {
        /// 通过 Animator 获取动画根运动的帧数据（位置和旋转变化），并通过委托机制允许外部代码处理这些数据
        rootMotionAction?.Invoke(animator.deltaPosition, animator.deltaRotation);

    }
    public void SetRootMotionAction(Action<Vector3,Quaternion> rootMotionAction)
    {
        this.rootMotionAction = rootMotionAction;
    }
    public void CleanRootMotionAction()
    {
        this.rootMotionAction = null;
    }
#endif


    #region 动画事件
    private void FootStep()
    {

    }
    #endregion
}
