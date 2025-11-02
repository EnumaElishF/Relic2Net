using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    //写new是为了解决变量名称重复的提醒
    private new Collider collider;
    /// <summary>
    /// 存储已经攻击到的目标，避免重复攻击到同一个对象，因为做的每一帧都会有检测的方式
    /// </summary>
    private HashSet<IHitTarget> hitTargets = new HashSet<IHitTarget>();
    private Action<IHitTarget, Vector3> onHitAction;
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="layer">玩家的武器能打到怪，怪的武器能打到玩家</param>
    /// <param name="onHit"></param>
    public void Init(string layer, Action<IHitTarget, Vector3> onHit)
    {
        this.onHitAction = onHit;
        gameObject.layer = LayerMask.NameToLayer(layer);
        collider = GetComponent<Collider>();
        collider.enabled = false;
    }
    public void StartHit()
    {
        collider.enabled = true;
        //Debug.Log("开启检测");
    }
    public void StopHit()
    {
        collider.enabled = false;
        //Debug.Log("关闭检测");
        hitTargets.Clear();
    }
    //碰撞过程中，每帧调用一次
    private void OnTriggerStay(Collider other)
    {
        IHitTarget target = other.GetComponentInParent<IHitTarget>();
        if (target != null && !hitTargets.Contains(target))
        {
            hitTargets.Add(target);
            Vector3 point = other.ClosestPoint(transform.position); // 命中点
            onHitAction?.Invoke(target, point);
        }
    }
}
