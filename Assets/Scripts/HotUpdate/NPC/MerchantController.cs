#if !UNITY_SERVER||UNITY_EDITOR
using UnityEngine;

public class MerchantController : NPCControllerBase
{
    public override string nameKey => "商人";

    protected override void OnPlayerInteraction()
    {
        //交互
        PlayerManager.Instance.OpenShop(configName);
    }

}
#endif
