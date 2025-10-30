using JKFrame;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    //加载的时候通过对象池取
    //卸载的时候通过对象池放进去
    private void OnParticleSystemStopped()
    {
        this.GameObjectPushPool();
    }
}
