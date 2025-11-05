
using UnityEngine;

public interface ICharacterClientController
{
    public void OnNetworkSpawn();
    public void OnNetworkDespawn();
    public void StartSkill(int skillIndex);
    public void StartSkillHit();
    public void PlaySkillHitEffect(Vector3 point); //特效命中点
}