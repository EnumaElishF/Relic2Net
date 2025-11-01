using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour,IHitTarget
{
    public void BeHit(AttackData attackData)
    {
        Debug.Log("我被打了");
    }

}
