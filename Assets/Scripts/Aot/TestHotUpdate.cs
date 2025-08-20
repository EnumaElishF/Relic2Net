using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
//启动的逻辑
public class TestHotUpdate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("开始");
        TextAsset dllText = Addressables.LoadAssetAsync<TextAsset>("HotUpdate.dll").WaitForCompletion();
        System.Reflection.Assembly.Load(dllText.bytes);
        Addressables.InstantiateAsync("GameObject").WaitForCompletion();
        Debug.Log("结束");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
