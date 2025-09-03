#if UNITY_SERVER || UNITY_EDITOR
using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public override void Enter()
    {
        player.PlayAnimation("Move");
    }
    public override void Update()
    {
        if(player.inputData.moveDir == Vector2.zero)
        {
            player.ChangeState(PlayerState.Idle);
            return;
        }

        //玩家开始移动

        player.transform.Translate(Time.deltaTime * player.MoveSpeed * player.inputData.moveDir);
        Vector2Int newCoord = AOIUtility.GetCoordByWorldPostion(player.transform.position);
        Vector2Int oldCoord = player.currentAOICoord;
        if (newCoord != oldCoord) //发生了地图块的坐标变化
        {
            AOIUtility.UpdatePlayerCoord(player, oldCoord, newCoord);
            player.currentAOICoord = newCoord;
        }
    }
}
#endif 