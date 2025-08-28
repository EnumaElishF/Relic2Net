using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
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
        public void Load(Vector2Int coord)
        {
            Vector2Int resCoord = coord + ClientMapManager.Instance.mapConfig.terrainResKeyCoordOffset;
            //按照Addressables的map地图文件名称设计，地图块编号_地图块编号
            string resKey =  $"{resCoord.x}_{resCoord.y}";
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
            }, ClientMapManager.Instance.transform);
        }
        public bool TryDestroy()
        {
            if(state == TerrainState.Disable)
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
        public void Destroy()
        {
            ResSystem.UnloadInstance(terrain.gameObject);
            destroyTimer = 0;
            terrain = null;
            this.ObjectPushPool();
        }
    }

    [SerializeField] private MapConfig mapConfig;
    public MapConfig MapConfig { get => mapConfig; }
    public float destroyTerrainTime;
    private QuadTree quadTree;


    protected override void Awake()
    {
        base.Awake();
        quadTree = new QuadTree(mapConfig);
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
