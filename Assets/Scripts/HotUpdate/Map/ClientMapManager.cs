using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JKFrame;
public class ClientMapManager : SingletonMono<ClientMapManager>
{
    [SerializeField] private MapConfig mapConfig;
    private QuadTree quadTree;
    protected override void Awake()
    {
        base.Awake();
        quadTree = new QuadTree(mapConfig);
    }

# if UNITY_EDITOR
    public bool drawGizmos;
    private void OnDrawGizmos()
    {
        if (drawGizmos && quadTree != null)
        {
            quadTree?.Draw();
        }
    }
#endif 
}
