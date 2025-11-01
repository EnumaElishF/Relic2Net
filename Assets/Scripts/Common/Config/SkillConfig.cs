using JKFrame;
using UnityEngine;
[CreateAssetMenu(menuName = "Config/Skill")]
public class SkillConfig : ConfigBase
{
    public string animationName;
    public float attackValueMultiple;//技能攻击力系数
    public float rotateNormalizedTime = 0.2f; //0.2的进度，序列化的时间,不是读秒的时间
    public float endNormalizedTime = 0.9f; 
    public float switchNormalizedTime = 0.5f;
    public float attackValue;
    public SkillEffect releaseEffect;//释放效果
    public SkillEffect startHitEffect;//开始命中效果
    public float repelDistance; //击退距离
    public float repelTime; //击退时间
}
public class SkillEffect
{
    public AudioClip audio;
    public GameObject prefab;
    public Vector3 offset;//偏移坐标
    public Vector3 rotation;//偏移角度
    public Vector3 scale;
}