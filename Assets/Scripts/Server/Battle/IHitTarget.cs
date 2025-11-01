public interface IHitTarget
{
    // 返回值代表这一次被攻击是否被击杀
    public void BeHit(AttackData attackData);
}

public struct AttackData
{
    public float attackValue; //攻击力
    public float repelDistance; //击退距离
    public float repelTime; //击退时间
    public UnityEngine.Vector3 sourcePosition;
}
