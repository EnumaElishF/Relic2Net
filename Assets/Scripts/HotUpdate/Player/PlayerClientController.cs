using JKFrame;
using UnityEngine;

public class PlayerClientController : MonoBehaviour
{
    /// <summary>
    /// 本地玩家，主玩家控制器
    /// </summary>
    private PlayerController mainController;
    private PlayerFloatInfo floatInfo;
    public void Init(PlayerController newPlayer)
    {
        this.mainController = newPlayer;
        if (mainController.IsSpawned) //本地玩家看不到自己的名字
        {
            if (floatInfo != null) floatInfo.gameObject.SetActive(false);
        }
        else
        {
            if (floatInfo == null) floatInfo = ResSystem.InstantiateGameObject<PlayerFloatInfo>(mainController.floatInfoPoint, "PlayerFloatInfo");
            floatInfo.Init(mainController.playerName.Value.ToString());
        }
    }
    
}
