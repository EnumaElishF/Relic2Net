#if !UNITY_SERVER||UNITY_EDITOR
using JKFrame;

public class CrafterController : NPCControllerBase
{
    public override string nameKey => "工匠";

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