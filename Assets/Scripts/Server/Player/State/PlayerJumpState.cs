using System;
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();//可以不保留，因为Enter里没有多少逻辑，如果父方法Enter内含重要逻辑的话，还是base一下比较好
        serverController.PlayAnimation("JumpStart");
        serverController.mainController.view.SetRootMotionAction(OnRootMotion);
        serverController.mainController.view.SetJumpStartEndAction(OnJumpStartEnd);
    }

    private void OnJumpStartEnd()
    {
        //玩家跳到最高点后开始下降
        serverController.ChangeState(PlayerState.AirDown);
    }

    public override void Exit()
    {
        mainController.view.CleanRootMotionAction();
    }

    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        //作为上升的情况，只会有正数，且不计算重力，只会变更更高
        deltaPosition.y *= serverController.jumpHeightMultiply;
        //deltaPosition.y -= 9.8f * Time.deltaTime;  //模拟重力:起跳不要模拟重力，不然跳不起来
        serverController.characterController.Move(deltaPosition);
        //更新AOI :因为起跳没有位移，所以不更新AOI
        //if (deltaPosition != Vector3.zero)
        //{
        //    serverController.UpdateAOICoord();
        //}
    }
}
