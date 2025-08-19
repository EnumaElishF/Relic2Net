using JKFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ClientLaunch : MonoBehaviour
{
    void Start()
    {
        GetComponent<HotUpdateSystem>().StartHotUpdate(null, (bool succeed) =>
        {
            if (succeed)
            {
                Addressables.InstantiateAsync("GameObject").WaitForCompletion();
            }
        });
    }


}
