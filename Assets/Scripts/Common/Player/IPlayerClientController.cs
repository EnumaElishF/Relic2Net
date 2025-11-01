using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IPlayerClientController
{
    public void OnNetworkSpawn();
    public void OnNetworkDespawn();
    public void StartSkill(int skillIndex);
    public void StartSkillHit();
    public void PlaySkillHitEffect(Vector3 point); //特效命中点
}