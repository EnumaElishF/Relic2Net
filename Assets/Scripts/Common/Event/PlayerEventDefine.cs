using UnityEngine;


public struct OnSpawnPlayerEvent
{
    public PlayerController newPlayer;
}
public struct AOIAddPlayerEvent
{
    public PlayerController player;
    public Vector2Int coord; //坐标点
}
public struct AOIUpdatePlayerCoordEvent
{
    public PlayerController player;
    public Vector2Int oldCoord; //旧坐标点（或者当前坐标点）
    public Vector2Int newCoord; //新坐标点
}

public struct AOIRemovePlayerEvent
{
    public PlayerController player;
    public Vector2Int coord; 

}
