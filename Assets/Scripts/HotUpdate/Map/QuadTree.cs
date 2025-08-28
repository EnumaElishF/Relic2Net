using System;
using UnityEngine;

public class QuadTree
{
    private static MapConfig mapConfig;
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

            if (isTerrain)
            {
                Debug.Log("输出1600次:");
            }

            if (divide && bounds.size.x > mapConfig.minQuadTreeNodeSize)
            {
                Divide();
            }
        }
        /// <summary>
        /// 逐层分开，每次分四个格子，这样完成四叉树
        /// </summary>
        private void Divide()
        {
            float halfSize = bounds.size.x / 2;
            float posOffset = halfSize / 2;
            float halfHeight = mapConfig.terrainMaxHeight / 2;
            Vector3 childSize = new Vector3(halfSize, mapConfig.terrainMaxHeight, halfSize);
            //这样就构建出来了四个格子，左上，右上，左下，右下
            leftAndTop = new Node(new Bounds(new Vector3(bounds.center.x - posOffset, halfHeight, bounds.center.z + posOffset), childSize), true);
            rightAndTop = new Node(new Bounds(new Vector3(bounds.center.x + posOffset, halfHeight, bounds.center.z + posOffset), childSize), true);
            leftAndBottom = new Node(new Bounds(new Vector3(bounds.center.x - posOffset, halfHeight, bounds.center.z - posOffset), childSize), true);
            rightAndBottom = new Node(new Bounds(new Vector3(bounds.center.x + posOffset, halfHeight, bounds.center.z - posOffset), childSize), true);
        }
        private bool CheckTerrain(out Vector2Int coord)
        {
            Vector3 size = bounds.size;
            //! 2d和3d参数在一起的时候，经常容易犯的低级错误，该写z的地方，容易写成y（错误），一定注意
            bool isTerrain = size.x == mapConfig.terrainSize && size.z == mapConfig.terrainSize;
            coord = Vector2Int.zero;
            if (isTerrain)
            {
                coord.x = (int)(bounds.center.x / mapConfig.terrainSize);
                coord.y = (int)(bounds.center.z / mapConfig.terrainSize); //因为是2d的所以用z
                //剔除mapSize的符合尺寸以外的terrain
                int maxCoordAbsX = (int)(mapConfig.mapSize.x / mapConfig.terrainSize) / 2;
                int maxCoordAbsY = (int)(mapConfig.mapSize.y / mapConfig.terrainSize) / 2;
                isTerrain = Mathf.Abs(coord.x) < maxCoordAbsX && Mathf.Abs(coord.y) < maxCoordAbsY;
            }
            return isTerrain;
        }

#if UNITY_EDITOR
        public void Draw()
        {
            Gizmos.color = isTerrain ? Color.green : Color.white;
            Gizmos.DrawWireCube(bounds.center, bounds.size * 1);
            //绘制完后把颜色改回去
            Gizmos.color = Color.white;


            leftAndTop?.Draw();
            rightAndTop?.Draw();
            leftAndBottom?.Draw();
            rightAndBottom?.Draw();
        }
#endif

    }

    private Node rootNode;
    public QuadTree(MapConfig config)
    {
        mapConfig = config;
        //这样就是一个19200*19200*200的立方体盒子
        Bounds rootBounds = new Bounds(new Vector3(0, mapConfig.terrainMaxHeight / 2, 0), new Vector3(mapConfig.quadTreeSize.x, mapConfig.terrainMaxHeight, mapConfig.quadTreeSize.y));

        rootNode = new Node(rootBounds, true);
    }
#if UNITY_EDITOR

    public void Draw()
    {
        rootNode?.Draw();
    }
#endif

}
