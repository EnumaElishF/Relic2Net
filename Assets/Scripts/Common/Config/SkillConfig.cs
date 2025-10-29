using JKFrame;
using UnityEngine;
[CreateAssetMenu(menuName = "Config/Skill")]
public class SkillConfig : ConfigBase
{
    public string animationName;
    public float attackValue;
    public float endTime = 0.9f;
    public float switchTime = 0.5f;
}
