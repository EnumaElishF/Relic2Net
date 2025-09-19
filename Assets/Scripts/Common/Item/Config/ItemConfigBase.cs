using UnityEngine;

public abstract class ItemConfigBase : ScriptableObject
{
#if !UNITY_SERVER || UNITY_EDITOR
    public Sprite icon;

#endif
}
