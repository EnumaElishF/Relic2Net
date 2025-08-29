using JKFrame;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public class ServerMapManager : MonoBehaviour
{
    //ServerMapManager���������������ò����ϣ�Ҫ��ClientMapManagerͳһ�ȽϺ�

    [SerializeField] private MapConfig mapConfig;

    void Start()
    {
        //���ݵ�ͼ���ã������첽���ز�ʵ�������е�ͼ�������飨Terrain���������������������е���ȷλ��
        //�Ӷ�ʵ�� ��һ���Լ���ȫ����ͼ�� ��Ч��
        int width = (int)(mapConfig.mapSize.x / mapConfig.terrainSize);
        int height = (int)(mapConfig.mapSize.y / mapConfig.terrainSize);
        //ͨ��Ƕ�� for ѭ���������е����������Դ���꣨resCoord������Χ�� (0,0) �� (width-1, height-1)��
        for (int x = 0; x < width; x++)
        {
            for(int y=0;y< height; y++)
            {
                Vector2Int resCoord = new Vector2Int(x, y);
                string resKey = $"{resCoord.x}_{resCoord.y}";
                //��Lambda���ʽ��дһ���ص�����
                ResSystem.InstantiateGameObjectAsync<Terrain>(resKey, (terrain) =>
                {
                    //����˵�terrain���Ҫһ������1600��terrain��Ⱦ���������Ը������أ����Բ����鿪����������ҷ����Ҳû��ʲô��Ҫ
                    terrain.enabled = false;
                    Vector2Int terrainCoord = resCoord - mapConfig.terrainResKeyCoordOffset;
                    //��������
                    terrain.transform.position = new Vector3(terrainCoord.x * mapConfig.terrainSize, 0, terrainCoord.y * mapConfig.terrainSize);

                }, transform, null, false); //false�ر��Զ��ͷ�
            }
        }
    }


}
