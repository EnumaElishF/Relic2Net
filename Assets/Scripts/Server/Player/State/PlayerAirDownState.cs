

using System;
using UnityEngine;

public class PlayerAirDownState: PlayerStateBase
{
    private bool onEndAnimation;
    //落地踩到地面的一瞬间要播放一个向下缓冲的动画
    public override void Enter()
    {
        onEndAnimation = false;
        serverController.PlayAnimation("JumpLoop");
    }
    public override void Update()
    {
        if (onEndAnimation)
        {
            if (serverController.inputData.moveDir != Vector3.zero)
            {
                serverController.ChangeState(PlayerState.Move);
                return;
            }
        } 
        else
        {
            Vector3 inputDir = serverController.inputData.moveDir.normalized * serverController.airMoveSpeed;
            Vector3 deltaPosition = new Vector3(inputDir.x * serverController.airMoveSpeed, serverController.gravity, inputDir.z);
            if (inputDir != Vector3.zero)
            {
                mainController.view.transform.rotation = Quaternion.RotateTowards(mainController.view.transform.rotation, Quaternion.LookRotation(inputDir), Time.deltaTime * serverController.rotateSpeed);
            }
            serverController.characterController.Move(deltaPosition * Time.deltaTime);
            if (serverController.characterController.isGrounded)
            {
                serverController.PlayAnimation("JumpEnd");
                onEndAnimation = true;
                mainController.view.jumpEndAction += OnJumpEnd;
            }
        }

    }
    public override void Exit()
    {
        mainController.view.jumpEndAction -= OnJumpEnd;
    }
    private void OnJumpEnd()
    {
        serverController.ChangeState(serverController.inputData.moveDir == Vector3.zero ? PlayerState.Idle : PlayerState.Move);
    }
}
