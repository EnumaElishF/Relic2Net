using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public override void Enter()
    {
        serverController.PlayAnimation("Move");
        serverController.mainController.View.SetRootMotionAction(OnRootMotion);
    }


    public override void Update()
    {
        if(serverController.inputData.moveDir == Vector3.zero)
        {
            serverController.ChangeState(PlayerState.Idle);
            return;
        }
        //旋转
        mainController.View.transform.rotation = Quaternion.RotateTowards(mainController.View.transform.rotation, Quaternion.LookRotation(serverController.inputData.moveDir), Time.deltaTime * mainController.RotateSpeed);
    }
    public override void Exit()
    {
        mainController.View.CleanRootMotionAction();
    }
    /// <summary>
    /// 必须检查动画Clip 是否启用了Root Motion，需要单独修改移动动画，否则出问题
    /// </summary>
    private void OnRootMotion(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        serverController.Animator.speed = mainController.MoveSpeed;
        deltaPosition.y -= 9.8f * Time.deltaTime;  //模拟重力
        serverController.CharacterController.Move(deltaPosition);
        //更新AOI :因为有位移，就要更新AOI
        if (deltaPosition != Vector3.zero)
        {
            serverController.UpdateAOICoord();
        }
    }

}
