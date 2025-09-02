using UnityEngine;

public class ClientGameSceneManager : MonoBehaviour
{
    void Start()
    {
        ClientMapManager.Instance.Init();//先初始化地图更合理一些

        PlayerManager.Instance.Init();
    }


}
