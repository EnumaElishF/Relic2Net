using JKFrame;
using System.Collections.Generic;
using UnityEngine;
public class ClientMapManager : SingletonMono<ClientMapManager>
{
    public enum TerrainState
    {
        Request,Enable,Disable
    }
    public class TerrainController
    {

        public Terrain terrain;
        public TerrainState state;
        public float destroyTimer;

        public void Enable()
        {
            if (state != TerrainState.Enable)
            {
                destroyTimer = 0;
                state = TerrainState.Enable;
                if (terrain != null)
                {
                    terrain.gameObject.SetActive(true);
                }
            }
        }
        public void Disable()
        {
            if (state != TerrainState.Disable)
            {
                state = TerrainState.Disable;
                if (terrain != null)
                {
                    terrain.gameObject.SetActive(false);
                }
            }
        }

        public void Load(Vector2Int coord)
        {
            Vector2Int resCoord = coord + ClientMapManager.Instance.mapConfig.terrainResKeyCoordOffset;
            //按照Addressables的map地图文件名称设计，地图块编号_地图块编号
            string resKey =  $"{resCoord.x}_{resCoord.y}";
            state = TerrainState.Request;
            ResSystem.InstantiateGameObjectAsync<Terrain>(resKey, (terrain) =>
            {
                this.terrain = terrain;
                //下面设置都是关于性能方面的东西
                terrain.basemapDistance = 100;
                terrain.heightmapPixelError = 50;
                terrain.heightmapMaximumLOD = 1;
                terrain.detailObjectDensity = 0.9f;
                terrain.treeDistance = 10;
                terrain.treeCrossFadeLength = 10;
                terrain.treeMaximumFullLODCount = 10;
                //得到terrain的正确尺寸
                terrain.transform.position = new Vector3(coord.x * Instance.mapConfig.terrainSize, 0, coord.y * Instance.mapConfig.terrainSize);
                if (state == TerrainState.Disable)
                {
                    terrain.gameObject.SetActive(false);
                }
            }, ClientMapManager.Instance.transform, null, false); //false关闭自动释放
        }
        public bool CheckAndDestroy()
        {
            if (state == TerrainState.Disable)
            {
                destroyTimer += Time.deltaTime;
                if (destroyTimer >= Instance.destroyTerrainTime)
                {
                    Destroy();
                    return true;
                }
            }
            return false;
        }
        public void Destroy()
        {
            if (terrain != null)
            {
                ResSystem.UnloadInstance(terrain.gameObject);
            }

            destroyTimer = 0;
            terrain = null;
            this.ObjectPushPool();
        }
    }

    [SerializeField] private MapConfig mapConfig;
    [SerializeField] private new Camera camera;
    public MapConfig MapConfig { get => mapConfig; }
    public float destroyTerrainTime;
    private QuadTree quadTree;
    private Dictionary<Vector2Int, TerrainController> terrainControllDic = new Dictionary<Vector2Int, TerrainController>(300);
    private List<Vector2Int> destroyTerrainCoords = new List<Vector2Int>(100);//销毁Terrain的坐标
    private Plane[] cameraPlanes = new Plane[6]; //相机的视椎体的所有面，有6个面对应6个Panel
    private Vector2Int playerTerrainCoord;
    public void Init()
    {
        //四叉树
        quadTree = new QuadTree(mapConfig,EnableTerrain,DisableTerrain,CheckVisibility);
    }

    private void Update()
    {
        if (camera == null) return;

        //相机,面
        GeometryUtility.CalculateFrustumPlanes(camera, cameraPlanes);

        //玩家坐标的问题：确保加载玩家的当前所在块 (每一帧玩家的位置会变，需要确保玩家当前所在的块，是被优先加载进来的）
        quadTree.CheckVisibility();

        //玩家当前坐标所在地图块
        playerTerrainCoord = GetTerrainCoordByWorldPos(camera.transform.position);
        EnableTerrain(playerTerrainCoord);//玩家坐标穿给Enable需要的Terrain


        //Terrain的管理
        foreach (KeyValuePair<Vector2Int, TerrainController> item in terrainControllDic)
        {
            if (item.Value.CheckAndDestroy())
            {
                destroyTerrainCoords.Add(item.Key);
            }
        }
        foreach(var item in destroyTerrainCoords)
        {
            terrainControllDic.Remove(item);
        }
        destroyTerrainCoords.Clear();
    }
    //外界告诉要销毁他
    private void DisableTerrain(Vector2Int coord)
    {
        if(terrainControllDic.TryGetValue(coord,out TerrainController controller))
        {
            controller.Disable();
        }
    }
    private void EnableTerrain(Vector2Int coord)
    {
        if (!terrainControllDic.TryGetValue(coord, out TerrainController controller))
        {
            controller = ResSystem.GetOrNew<TerrainController>();
            controller.Load(coord);
            terrainControllDic.Add(coord, controller);
        }
        controller.Enable();
    }

    private Vector2Int GetTerrainCoordByWorldPos(Vector3 worldPos)
    {
        return new Vector2Int((int)(worldPos.x / mapConfig.terrainSize), (int)(worldPos.z / mapConfig.terrainSize));
    }

    private Vector3 GetWorldPosByTerrainCoord(Vector2Int coord)
    {
        return new Vector3(coord.x * mapConfig.terrainSize, 0, coord.y * mapConfig.terrainSize);
    }

    private bool CheckVisibility(Bounds bounds)
    {
        //希望实际的可见范围大一些,（因为使用异步加载，让加载的范围大一些，防止真的摄像机看的时候资源还没加载好
        bounds.size *= 2;

        if (GeometryUtility.TestPlanesAABB(cameraPlanes, bounds)) return true;
        // 玩家当前地块附近的地块要显示：（另外这里的bounds.size因为传的时候乘2了，所以玩家周围地块会超过九宫格，范围变大
        Vector3 boundsCenter = GetWorldPosByTerrainCoord(playerTerrainCoord);
        Bounds playerTerrainBounds = new Bounds(boundsCenter, new Vector3(mapConfig.terrainSize, mapConfig.terrainMaxHeight, mapConfig.terrainSize) * 3);//xz都*3倍，变为九宫格的样式
        return bounds.Intersects(playerTerrainBounds);
    }

# if UNITY_EDITOR
    public bool drawGizmos;
    private void OnDrawGizmos()
    {
        if (drawGizmos && quadTree != null)
        {
            quadTree?.Draw();
        }
    }
#endif 
}
