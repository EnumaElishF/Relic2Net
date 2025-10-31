public interface IHitTarget
{
    // 返回值代表这一次被攻击是否被击杀
    public bool BeHit(AttackData attackData);
}

public struct AttackData
{
    public float attackValue;
    public float repelDistance;
    public float repelTime;
    public UnityEngine.Vector3 sourcePosition;
}
