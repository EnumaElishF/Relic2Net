using System;
using UnityEngine;

public class QuadTree
{
    private static MapConfig mapConfig;
    private static Action<Vector2Int> onTerrainEnable;
    private static Action<Vector2Int> onTerrainDisable;
    private static Func<Bounds,bool> onCheckVisibility; //���а�Χ�в���Bounds�ķ���ֵΪbool�� ί��
    private class Node
    {
        public Bounds bounds;
        private Node leftAndTop; 
        private Node rightAndTop;
        private Node leftAndBottom;
        private Node rightAndBottom;
        private bool isTerrain;
        private Vector2Int terrainCoord;


        public Node(Bounds bounds,bool divide)
        {
            this.bounds = bounds;
            isTerrain = CheckTerrain(out terrainCoord);

            //if (isTerrain)
            //{
            //    Debug.Log("���1600��:");
            //}

            if (divide && bounds.size.x > mapConfig.minQuadTreeNodeSize)
            {
                Divide();
            }
        }
        /// <summary>
        /// ���ֿ���ÿ�η��ĸ����ӣ���������Ĳ���
        /// </summary>
        private void Divide()
        {
            float halfSize = bounds.size.x / 2;
            float posOffset = halfSize / 2;
            float halfHeight = mapConfig.terrainMaxHeight / 2;
            Vector3 childSize = new Vector3(halfSize, mapConfig.terrainMaxHeight, halfSize);
            //�����͹����������ĸ����ӣ����ϣ����ϣ����£�����
            leftAndTop = new Node(new Bounds(new Vector3(bounds.center.x - posOffset, halfHeight, bounds.center.z + posOffset), childSize), true);
            rightAndTop = new Node(new Bounds(new Vector3(bounds.center.x + posOffset, halfHeight, bounds.center.z + posOffset), childSize), true);
            leftAndBottom = new Node(new Bounds(new Vector3(bounds.center.x - posOffset, halfHeight, bounds.center.z - posOffset), childSize), true);
            rightAndBottom = new Node(new Bounds(new Vector3(bounds.center.x + posOffset, halfHeight, bounds.center.z - posOffset), childSize), true);
        }
        private bool CheckTerrain(out Vector2Int coord)
        {
            Vector3 size = bounds.size;
            //! 2d��3d������һ���ʱ�򣬾������׷��ĵͼ����󣬸�дz�ĵط�������д��y�����󣩣�һ��ע��
            bool isTerrain = size.x == mapConfig.terrainSize && size.z == mapConfig.terrainSize;
            coord = Vector2Int.zero;
            if (isTerrain)
            {
                coord.x = (int)(bounds.center.x / mapConfig.terrainSize);
                coord.y = (int)(bounds.center.z / mapConfig.terrainSize); //��Ϊ��2d��������z
                //�޳�mapSize�ķ��ϳߴ������terrain
                int maxCoordAbsX = (int)(mapConfig.mapSize.x / mapConfig.terrainSize) / 2;
                int maxCoordAbsY = (int)(mapConfig.mapSize.y / mapConfig.terrainSize) / 2;
                isTerrain = Mathf.Abs(coord.x) < maxCoordAbsX && Mathf.Abs(coord.y) < maxCoordAbsY;
            }
            return isTerrain;
        }

        private bool active = false;

        public void CheckVisibility()
        {
            bool newActiveState = onCheckVisibility(bounds);
            // ���� �����ǰ�ڵ� == �ɽڵ㣬�Ǿ�����û���������û�仯�������ӽڵ���һ����ԭ�����ļ������ڿ�����
            // ��ԭ���ɼ������ڲ��ɼ�����һ����Ҫȫ��Disable���ġ���������ӽڵ��б仯����������Ϊ���ڵ�û�б仯����Ҫ���뿼��

            //��1�� ԭ���ɼ������ڿɼ�  ��2�� ԭ�����ɼ������ڿɼ�  
            // (active && newActiveState) �� (!active && newActiveState) ��ʵ�����������,����Ҫ���ǽ��� ��ȵݹ��
            if (newActiveState)
            {
                // ԭ���ɼ������ڿɼ� :�Ǿ�ȫ����һ���飬��ȵݹ�
                if (isTerrain) onTerrainEnable.Invoke(terrainCoord);
                else
                {
                    leftAndTop?.CheckVisibility();
                    rightAndTop?.CheckVisibility();
                    leftAndBottom?.CheckVisibility();
                    rightAndBottom?.CheckVisibility();
                }
            }
            else if (active && !newActiveState) // ԭ���ɼ������ڲ��ɼ�
            {
                Disable();
            }
            //���գ��ݹ鶼�����ˣ�����active������ά��
            active = newActiveState;
        }
        public void Disable()
        {
            //�ݹ飬����ǲ���ҪDisable
            leftAndTop?.Disable();
            rightAndTop?.Disable();
            leftAndBottom?.Disable();
            rightAndBottom?.Disable();
            if (isTerrain)
            {
                //�����Terrain����Ҫ��������ȥDisable����������ҪterrainCoord���꣩
                onTerrainDisable.Invoke(terrainCoord);
            }
            //�������Terrain���Ǿ�ʲô�����ù��ˣ���ǰ�ͱ��رյ���
        }

#if UNITY_EDITOR
        public void Draw()
        {
            Gizmos.color = isTerrain ? Color.green : Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size * 1);
            //����������ɫ�Ļ�ȥ
            Gizmos.color = Color.white;


            leftAndTop?.Draw();
            rightAndTop?.Draw();
            leftAndBottom?.Draw();
            rightAndBottom?.Draw();
        }
#endif

    }

    private Node rootNode;
    public QuadTree(MapConfig config, Action<Vector2Int> terrainEnable, Action<Vector2Int> terrainDisable, Func<Bounds, bool> checkVisibility)
    {
        mapConfig = config;
        onTerrainEnable = terrainEnable;
        onTerrainDisable = terrainDisable;
        onCheckVisibility = checkVisibility;

        //��������һ��19200*19200*200�����������
        Bounds rootBounds = new Bounds(new Vector3(0, mapConfig.terrainMaxHeight / 2, 0), new Vector3(mapConfig.quadTreeSize.x, mapConfig.terrainMaxHeight, mapConfig.quadTreeSize.y));

        rootNode = new Node(rootBounds, true);
    }

    public void CheckVisibility()
    {
        rootNode.CheckVisibility();
    }
#if UNITY_EDITOR

    public void Draw()
    {
        rootNode?.Draw();
    }
#endif

}
