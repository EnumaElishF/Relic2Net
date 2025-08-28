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
            if (state == TerrainState.Disable)
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
            //����Addressables��map��ͼ�ļ�������ƣ���ͼ����_��ͼ����
            string resKey =  $"{resCoord.x}_{resCoord.y}";
            state = TerrainState.Request;
            ResSystem.InstantiateGameObjectAsync<Terrain>(resKey, (terrain) =>
            {
                this.terrain = terrain;
                //�������ö��ǹ������ܷ���Ķ���
                terrain.basemapDistance = 100;
                terrain.heightmapPixelError = 50;
                terrain.heightmapMaximumLOD = 1;
                terrain.detailObjectDensity = 0.9f;
                terrain.treeDistance = 10;
                terrain.treeCrossFadeLength = 10;
                terrain.treeMaximumFullLODCount = 10;
                //�õ�terrain����ȷ�ߴ�
                terrain.transform.position = new Vector3(coord.x * Instance.mapConfig.terrainSize, 0, coord.y * Instance.mapConfig.terrainSize);
                if (state == TerrainState.Disable)
                {
                    terrain.gameObject.SetActive(false);
                }
            }, ClientMapManager.Instance.transform);
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
        public void Disable()
        {
            if (state == TerrainState.Enable)
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
    [SerializeField] private new Camera camera;
    public MapConfig MapConfig { get => mapConfig; }
    public float destroyTerrainTime;
    private QuadTree quadTree;
    private Dictionary<Vector2Int, TerrainController> terrainControllDic = new Dictionary<Vector2Int, TerrainController>(300);
    private List<Vector2Int> destroyTerrainCoords = new List<Vector2Int>(100);//����Terrain������
    private Plane[] cameraPlanes; //�������׵��������棬��6�����Ӧ6��Panel
    private Vector2Int playerTerrainCoord;
    protected override void Awake()
    {
        base.Awake();
        //�Ĳ���
        quadTree = new QuadTree(mapConfig,EnableTerrain,DisableTerrain,CheckVisibility);
    }

    private void Update()
    {
        //���,��
        GeometryUtility.CalculateFrustumPlanes(camera, cameraPlanes);

        //�����������⣺ȷ��������ҵĵ�ǰ���ڿ� (ÿһ֡��ҵ�λ�û�䣬��Ҫȷ����ҵ�ǰ���ڵĿ飬�Ǳ����ȼ��ؽ����ģ�
        quadTree.CheckVisibility();
        if (camera != null)
        {
            //��ҵ�ǰ�������ڵ�ͼ��
            playerTerrainCoord = GetTerrainCoordByWorldPos(camera.transform.position);
            EnableTerrain(playerTerrainCoord);//������괩��Enable��Ҫ��Terrain

        }


        //Terrain�Ĺ���
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
    //������Ҫ������
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
        controller.Disable();
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
        //ϣ��ʵ�ʵĿɼ���Χ��һЩ,����Ϊʹ���첽���أ��ü��صķ�Χ��һЩ����ֹ������������ʱ����Դ��û���غ�
        bounds.size *= 2;

        if (GeometryUtility.TestPlanesAABB(cameraPlanes, bounds)) return true;
        // ��ҵ�ǰ�ؿ鸽���ĵؿ�Ҫ��ʾ�������������bounds.size��Ϊ����ʱ���2�ˣ����������Χ�ؿ�ᳬ���Ź��񣬷�Χ���
        Vector3 boundsCenter = GetWorldPosByTerrainCoord(playerTerrainCoord);
        Bounds playerTerrainBounds = new Bounds(boundsCenter, new Vector3(mapConfig.terrainSize, mapConfig.terrainMaxHeight, mapConfig.terrainSize) * 3);//xz��*3������Ϊ�Ź������ʽ
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
