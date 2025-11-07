using JKFrame;
using UnityEngine;

public class MonsterClientController : CharacterClientControllerBase<MonsterController>, IMonsterClientController
{
    public Transform floatInfoPoint { get; private set; }
    private MonsterFloatInfo floatInfo;
    public override void FirstInit(MonsterController mainController)
    {
        base.FirstInit(mainController);
        floatInfoPoint = transform.Find("FloatPoint");
        floatInfo = ResSystem.InstantiateGameObject<MonsterFloatInfo>(floatInfoPoint,nameof(MonsterFloatInfo));
    }
    public override void Init()
    {
        floatInfo.Init(mainController.monsterConfig);
        OnHpChanged(0, mainController.currentHp.Value);
    }

    protected override void OnHpChanged(float previousValue, float newValue)
    {
        float fillAmount = newValue / mainController.monsterConfig.maxHP;
        floatInfo.UpdateHp(fillAmount);
    }
}
