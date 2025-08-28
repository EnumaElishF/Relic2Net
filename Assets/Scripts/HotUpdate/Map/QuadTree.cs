using System;
using UnityEngine;

public class QuadTree
{
    private static MapConfig mapConfig;
    private static Action<Vector2Int> onTerrainEnable;
    private static Action<Vector2Int> onTerrainDisable;
    private static Func<Bounds,bool> onCheckVisibility; //带有包围盒参数Bounds的返回值为bool的 委托
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
            //    Debug.Log("输出1600次:");
            //}

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

        private bool active = false;

        public void CheckVisibility()
        {
            bool newActiveState = onCheckVisibility(bounds);
            // 考虑 如果当前节点 == 旧节点，那就是他没变过，那他没变化，他的子节点有一部分原来看的见，现在看不到
            // 他原本可见，现在不可见，那一定是要全部Disable掉的。但是如果子节点有变化，但是他作为父节点没有变化，需要深入考虑

            //（1） 原本可见，现在可见  （2） 原本不可见，现在可见  
            // (active && newActiveState) 或 (!active && newActiveState) 其实就是两种情况,是需要考虑进行 深度递归的
            if (newActiveState)
            {
                // 原本可见，现在可见 :那就全部跑一遍检查，深度递归
                if (isTerrain) onTerrainEnable.Invoke(terrainCoord);
                else
                {
                    leftAndTop?.CheckVisibility();
                    rightAndTop?.CheckVisibility();
                    leftAndBottom?.CheckVisibility();
                    rightAndBottom?.CheckVisibility();
                }
            }
            else if (active && !newActiveState) // 原本可见，现在不可见
            {
                Disable();
            }
            //最终，递归都完事了，进行active变量的维护
            active = newActiveState;
        }
        public void Disable()
        {
            //递归，检查是不是要Disable
            leftAndTop?.Disable();
            rightAndTop?.Disable();
            leftAndBottom?.Disable();
            rightAndBottom?.Disable();
            if (isTerrain)
            {
                //如果是Terrain，需要告诉他，去Disable掉，（他需要terrainCoord坐标）
                onTerrainDisable.Invoke(terrainCoord);
            }
            //如果不是Terrain，那就什么都不用管了，当前就被关闭掉了
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
    public QuadTree(MapConfig config, Action<Vector2Int> terrainEnable, Action<Vector2Int> terrainDisable, Func<Bounds, bool> checkVisibility)
    {
        mapConfig = config;
        onTerrainEnable = terrainEnable;
        onTerrainDisable = terrainDisable;
        onCheckVisibility = checkVisibility;

        //这样就是一个19200*19200*200的立方体盒子
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
