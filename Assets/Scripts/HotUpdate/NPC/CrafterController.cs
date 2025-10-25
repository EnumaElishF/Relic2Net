#if !UNITY_SERVER||UNITY_EDITOR
using JKFrame;
using UnityEngine;

public class CrafterController : NPCControllerBase
{
    public override string nameKey => "工匠-达克内斯";

    protected override void OnPlayerInteraction()
    {
         PlayerManager.Instance.OpenCraft(configName);
    }
    //protected override void MainInteraction()
    //{
    //    PlayerManager.Instance.OpenCraft(configName);
    //}
}
#endif