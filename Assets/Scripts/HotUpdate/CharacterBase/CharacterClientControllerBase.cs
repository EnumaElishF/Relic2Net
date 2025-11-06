using JKFrame;
using System;
using UnityEngine;

public abstract class CharacterClientControllerBase<M> : MonoBehaviour, ICharacterClientController where M : CharacterControllerBase
{
    public M mainController { get; protected set; } //PlayerController + MonsterController
    protected SkillConfig currentSkillConfig;

    public virtual void FirstInit(M mainController) //第一次被添加组件的调用：记得virtual方便其下Player、Monster分别调用
    {
        this.mainController = mainController;
        mainController.ClientController = this;
    }
    public abstract void Init();
    #region 网络相关 ：生成和销毁
    public virtual void OnNetworkSpawn()
    {
        mainController.currentHp.OnValueChanged = OnHpChanged;
    }

    protected abstract void OnHpChanged(float previousValue, float newValue); //对于这个父类来说，OnHpChanged具体实现是不需要知道的，交给子类做吧

    public virtual void OnNetworkDespawn()
    {

    }
    #endregion
    #region 战斗
    public void StartSkill(int skillIndex)
    {
        currentSkillConfig = mainController.skillConfigList[skillIndex];
        PlaySkillEffect(currentSkillConfig.releaseEffect);
    }
    public void StartSkillHit()
    {
        PlaySkillEffect(currentSkillConfig.startHitEffect);
    }
    private void PlaySkillEffect(SkillEffect skillEffect)
    {
        if (skillEffect == null) return;
        if (skillEffect.audio != null)
        {
            AudioSystem.PlayOneShot(skillEffect.audio, transform.position);
        }
        if (skillEffect.prefab != null)
        {
            GameObject effectObj = GlobalUtility.GetOrInstantiate(skillEffect.prefab, null);
            //效果的坐标，他是要考虑坐标在套入角色内，以及单独拿出外，坐标是会变换的，毕竟父级不一样
            //效果的坐标，这里函数存的是对于在角色上是0,0,0的坐标，拉出来，无父级的时候是什么坐标。这样一个一直相对变化值
            //将「角色本地坐标系的偏移量」转换为「世界坐标系的绝对位置」，让效果贴合角色指定位置
            effectObj.transform.position = mainController.CharacterView.transform.TransformPoint(skillEffect.offset);
            //效果的旋转，道理和上面类似，但是容易一些，我们用四元数相乘的逻辑，做角度叠加。 相对偏移角度skillEffect.rotation
            //将「角色的基础旋转」与「效果的相对旋转」叠加，让效果朝向符合预期（如技能发射方向、特效朝向）
            effectObj.transform.rotation = mainController.CharacterView.transform.rotation * Quaternion.Euler(skillEffect.rotation);
            //因为角色模型不会变大变小，所以效果的相对缩放这个方面倒不用考虑其他计算，直接用设定就行
            effectObj.transform.localScale = skillEffect.scale;
        }
    }


    public void PlaySkillHitEffect(Vector3 point)
    {
        SkillEffect skillEffect = currentSkillConfig.hitEffect;
        if (skillEffect == null) return;
        if (skillEffect.audio != null)
        {
            AudioSystem.PlayOneShot(skillEffect.audio, transform.position);
        }
        if (skillEffect.prefab != null)
        {
            GameObject effectObj = GlobalUtility.GetOrInstantiate(skillEffect.prefab, null);
            effectObj.transform.position = point;
            effectObj.transform.localScale = skillEffect.scale;
        }
    }
    #endregion


}