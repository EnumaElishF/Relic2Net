using UnityEngine;
using JKFrame;
[CreateAssetMenu(menuName ="Config/MapConfig")]
public class MapConfig : ConfigBase
{
    //��ͼ��Ĭ������

    //�Ĳ����ĳߴ磬����õĳ����ͱȽϴ�300��Ϊ��С������ * �ָ�Ϊ4 * 4 * 4 = 19200
    public Vector2 quadTreeSize = new Vector2(19200, 19200);
    public Vector2 mapSize = new Vector2(12000, 12000); //ѡ��19200�����Ĳ����ߴ磬��ʵ�ʳߴ�Ҫ��һЩ��
    public Vector2Int terrainCoordOffset;
    public float terrainSize = 300; //Terrain�ĵ�������300*300�����,Terrain��Χ�е����߶Ⱦ���200
    public float terrainMaxHeight = 200f;
    public float minQuadTreeNodeSize = 300;

    private void OnValidate()
    {
        terrainCoordOffset = new Vector2Int(-(int)(mapSize.x / terrainSize / 2), -(int)(mapSize.y / terrainSize / 2));
    }
}
