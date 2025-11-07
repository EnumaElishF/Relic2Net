using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="Config/MonsterConfig")]
public class MonsterConfig : ConfigBase
{
    public Dictionary<LanguageType, string> nameDic;
    public float maxHP;
    public float attackValue;
    public float maxIdleTime;
    public float maxPatrolTime;
}
