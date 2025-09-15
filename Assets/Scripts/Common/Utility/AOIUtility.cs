using JKFrame;
using UnityEngine;
/// <summary>
/// 结合PlayerEventDefine事件控制，作为玩家角色与AOI的桥梁机制
/// </summary>
public static class AOIUtility 
{
    public static float chunkSize { get; private set; }
    public static void Init(float chunkSize)
    {
        AOIUtility.chunkSize = chunkSize;
    }
    public static Vector2Int GetCoordByWorldPostion(Vector3 worldPostion)
    {
        return new Vector2Int((int)(worldPostion.x / chunkSize), (int)(worldPostion.z / chunkSize));
    }
    public static void AddPlayer(PlayerController player,Vector2Int AOICoord)
    {
        Debug.Log("触发添加玩家");
        //类型事件的触发
        EventSystem.TypeEventTrigger(new AOIAddPlayerEvent 
        { player = player, 
          coord = AOICoord 
        });
    }
    public static void UpdatePlayerCoord(PlayerController player,Vector2Int oldCoord,Vector2Int newCoord)
    {
        Debug.Log("触发玩家坐标事件UpdatePlayerCoord");
        //类型事件的触发
        EventSystem.TypeEventTrigger(new AOIUpdatePlayerCoordEvent
        {
            player = player,
            oldCoord = oldCoord,
            newCoord = newCoord
        });

    }
    public static void RemovePlayer(PlayerController player, Vector2Int AOICoord)
    {
        Debug.Log("触发删除玩家");
        //类型事件的触发
        EventSystem.TypeEventTrigger(new AOIRemovePlayerEvent
        {
            player = player,
            coord = AOICoord
        });
    }
}
