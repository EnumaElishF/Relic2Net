#if UNITY_SERVER || UNITY_EDITOR
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public override void Enter()
    {
        serverController.PlayAnimation("Idle");
    }
    public override void Update()
    {
        if (serverController.inputData.moveDir != Vector3.zero)
        {
            serverController.ChangeState(PlayerState.Move);
            return;
        }
        if (!serverController.CharacterController.isGrounded)
        {
            //防止玩家角色在待机的时候，脚踩空导致飞起来，我们给他一个Idle的时候的重力。
            serverController.CharacterController.Move(new Vector3(0, -9.8f * Time.deltaTime, 0));
        }
    }
}
#endif