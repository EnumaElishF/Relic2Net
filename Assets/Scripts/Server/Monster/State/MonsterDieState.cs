using UnityEngine;

public class MonsterDieState : MonsterStateBase
{
    public override void Enter()
    {
        serverController.PlayAnimation("Die");
    }
    public override void Update()
    {
        if(serverController.CheckAnimationState("Die",out float normalizedTiem) && normalizedTiem >= 0.95f)
        {
            serverController.Die();
        }
    }
}