using JKFrame;
using UnityEngine;
[CreateAssetMenu(menuName = "Config/Skill")]
public class SkillConfig : ConfigBase
{
    public string animationName;
    public float attackValue;
    public float endTime = 0.9f;
    public float switchTime = 0.5f;
    public SkillEffect releaseEffect;//释放效果
    public SkillEffect startHitEffect;//开始命中效果
}
public class SkillEffect
{
    public AudioClip audio;
    public GameObject prefab;
    public Vector3 offset;//偏移坐标
    public Vector3 rotation;//偏移角度
    public Vector3 scale;
}