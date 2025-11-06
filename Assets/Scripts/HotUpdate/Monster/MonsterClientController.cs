using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterClientController : CharacterClientControllerBase<MonsterController>, IMonsterClientController
{
    public override void Init()
    {
        
    }

    protected override void OnHpChanged(float previousValue, float newValue)
    {
        
    }
}
