using JKFrame;
using UnityEngine;

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
        //类型事件的触发
        EventSystem.TypeEventTrigger(new AOIAddPlayerEvent 
        { player = player, 
          coord = AOICoord 
        });
    }
    public static void UpdatePlayerCoord(PlayerController player,Vector2Int oldCoord,Vector2Int newCoord)
    {
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
        //类型事件的触发
        EventSystem.TypeEventTrigger(new AOIRemovePlayerEvent
        {
            player = player,
            coord = AOICoord
        });
    }
}
