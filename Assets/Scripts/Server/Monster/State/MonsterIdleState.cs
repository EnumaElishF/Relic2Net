public class MonsterIdleState : MonsterStateBase
{
    public override void Enter()
    {
        serverController.PlayAnimation("Idle");
    }
}