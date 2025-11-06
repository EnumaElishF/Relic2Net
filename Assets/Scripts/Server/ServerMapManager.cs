using JKFrame;
using Unity.AI.Navigation;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public class ServerMapManager : SingletonMono<ServerMapManager>
{
    //ServerMapManager在制作基本的配置参数上，要和ClientMapManager统一比较好

    [SerializeField] private MapConfig mapConfig;
    [SerializeField] private NavMeshSurface navMeshSurface;

    //优化服务的加载全部地形，过滤掉周围一部分来提高测试速度
    [SerializeField] private int  testRange = 36; //过滤掉最外围几圈的地图块

    public void Init()
    {
        //根据地图配置，批量异步加载并实例化所有地图地形区块（Terrain），并设置它们在世界中的正确位置
        //从而实现 “一次性加载全部地图” 的效果
        int width = (int)(mapConfig.mapSize.x / mapConfig.terrainSize);
        int height = (int)(mapConfig.mapSize.y / mapConfig.terrainSize);

        
        //通过嵌套 for 循环遍历所有地形区块的资源坐标（resCoord），范围是 (0,0) 到 (width-1, height-1)：
        for (int x = testRange / 2; x < width - testRange / 2; x++)
        {
            //0~40 要testRange = 20; 那么就剩下10~30,干掉0~9,31~40。若testRange = 35，就只剩下5*5了,就会特别快
            for (int y = testRange / 2; y < height - testRange / 2; y++)
            {
                Vector2Int resCoord = new Vector2Int(x, y);
                string resKey = $"{resCoord.x}_{resCoord.y}";
                Vector2Int terrainCoord = resCoord - mapConfig.terrainResKeyCoordOffset;
                Vector3 pos = new Vector3(terrainCoord.x * mapConfig.terrainSize, 0, terrainCoord.y * mapConfig.terrainSize);
                ServerResSystem.InstantiateTerrain(resKey, transform, pos);

            }
        }
        //完成上面的实例化所有地图以后
        navMeshSurface.BuildNavMesh();
    }


}
