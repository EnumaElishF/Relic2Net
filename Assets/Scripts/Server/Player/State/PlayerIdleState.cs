using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public override void Enter()
    {
        serverController.PlayAnimation("Idle");
    }
    public override void Update()
    {
        if (serverController.inputData.attack)
        {
            serverController.ChangeState(PlayerState.Attack);
            return;
        }
        if (serverController.inputData.jump) //跳跃优先级做的比移动高一些
        {
            serverController.ChangeState(PlayerState.Jump);
            return;
        }
        if (serverController.inputData.moveDir != Vector3.zero)
        {
            serverController.ChangeState(PlayerState.Move);
            return;
        }
        if (!serverController.characterController.isGrounded)
        {
            //防止玩家角色在待机的时候，脚踩空导致飞起来，我们给他一个Idle的时候的重力。
            serverController.characterController.Move(new Vector3(0, -9.8f * Time.deltaTime, 0));
        }
    }
}
