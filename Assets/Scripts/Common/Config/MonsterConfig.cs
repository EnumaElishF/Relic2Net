using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="Config/MonsterConfig")]
public class MonsterConfig : ConfigBase
{
    public Dictionary<LanguageType, string> nameDic;
    public float maxHP;//最大生命值
    public float attackValue;//攻击力
    public float maxIdleTime;//最大待机时间
    public float maxPatrolTime;//最大巡逻时间
    public float searchPlayerRange;//搜索玩家的范围
    public float pursuitTime;//追击的时间

}
