using System;
using UnityEngine;

public class PlayerJumpState : PlayerStateBase
{
    public override void Enter()
    {
        base.Enter();//可以不保留，因为Enter里没有多少逻辑，如果父方法Enter内含重要逻辑的话，还是base一下比较好
        serverController.inputData.jump = false;
        serverController.PlayAnimation("JumpStart");
        serverController.mainController.view.rootMotionAction += OnRootMotion;
        serverController.mainController.view.jumpStartEndAction += OnJumpStartEnd;
    }

    private void OnJumpStartEnd()
    {
        //玩家跳到最高点后开始下降
        serverController.ChangeState(PlayerState.AirDown);
    }

    public override void Exit()
    {
        serverController.mainController.view.rootMotionAction -= OnRootMotion;
        serverController.mainController.view.jumpStartEndAction -= OnJumpStartEnd;
    }
    /// <summary>
    /// OnRootMotion只考虑了y轴
    /// </summary>
    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        //应用一个Y轴上升的系数
        //作为上升的情况，只会有正数，且不计算重力，只会变更更高
        deltaPosition.y *= serverController.jumpHeightMultiply;
        //deltaPosition.y -= 9.8f * Time.deltaTime;  //模拟重力:起跳不要模拟重力，不然跳不起来

        Vector3 moveDir = serverController.inputData.moveDir;
        if (moveDir != Vector3.zero)
        {
            //旋转:往哪边按就往哪边转的，设计（也可以修改成别的设计）
            mainController.view.transform.rotation = Quaternion.RotateTowards(mainController.view.transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * serverController.rotateSpeed);
            Vector3 forward = Time.deltaTime * serverController.rootMotionMoveSpeedMultiply * mainController.view.transform.forward;
            //应用玩家的输入情况，x和z轴的
            deltaPosition.x = forward.x;
            deltaPosition.z = forward.y;
        }
        serverController.characterController.Move(deltaPosition);
        //更新AOI :因为起跳没有位移，所以不更新AOI
        //if (deltaPosition != Vector3.zero)
        //{
        //    serverController.UpdateAOICoord();
        //}
    }
}
