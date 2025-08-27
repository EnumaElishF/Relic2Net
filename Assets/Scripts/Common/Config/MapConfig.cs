using UnityEngine;
using JKFrame;
[CreateAssetMenu(menuName ="Config/MapConfig")]
public class MapConfig : ConfigBase
{
    //地图的默认设置

    //四叉树的尺寸，这个用的场景就比较大，300作为最小颗粒度 * 分割为4 * 4 * 4 = 19200
    public Vector2 quadTreeSize = new Vector2(19200, 19200);
    public Vector2 mapSize = new Vector2(12000, 12000); //选择19200这种四叉树尺寸，比实际尺寸要大一些。
    public Vector2Int terrainCoordOffset;
    public float terrainSize = 300; //Terrain的单个就是300*300的面积,Terrain包围盒的最大高度就是200
    public float terrainMaxHeight = 200f;
    public float minQuadTreeNodeSize = 300;

    private void OnValidate()
    {
        terrainCoordOffset = new Vector2Int(-(int)(mapSize.x / terrainSize / 2), -(int)(mapSize.y / terrainSize / 2));
    }
}
