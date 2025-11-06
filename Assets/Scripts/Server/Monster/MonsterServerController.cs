using JKFrame;
using UnityEngine;

public class MonsterServerController : CharacterServerControllerBase<MonsterController>,IMonsterServerController,IStateMachineOwner
{
    protected override void OnInitAOI()
    {
        AOIManager.Instance.InitServerObject(mainController.NetworkObject, currentAOICoord);
    }
    protected override void OnUpdateAOI(Vector2Int newCoord)
    {
        AOIManager.Instance.UpdateServerObjectChunkCoord(mainController.NetworkObject, currentAOICoord, newCoord);
    }
    protected override void OnRemoveAOI()
    {
        AOIManager.Instance.RemoveServerObject(mainController.NetworkObject, currentAOICoord);
    }


}
