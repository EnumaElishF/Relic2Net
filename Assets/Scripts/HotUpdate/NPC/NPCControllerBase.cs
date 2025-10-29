using JKFrame;
using UnityEngine;

public abstract class NPCControllerBase : MonoBehaviour
{
    [SerializeField] protected string configName;
    //[SerializeField] protected DialogConfig defaultDialogConfig;
    //[SerializeField] protected string taskID;
    [SerializeField] protected GameObject prompt;
    [SerializeField] protected Transform floatInfoPoint; //能尽量往上抽一点，就往Base里放，毕竟同样的代码多写也没有意义
    public abstract string nameKey { get; }

    private void Start()
    {
        if (prompt != null) prompt.SetActive(false);
#if UNITY_EDITOR
        if (NetManager.Instance.IsServer) return;
#endif
        InitFloatInfo();
    }
    public void InitFloatInfo()
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsServer) return;
#endif
        NPCFloatInfo floatInfo = ResSystem.InstantiateGameObject<NPCFloatInfo>(floatInfoPoint, "NPCFloatInfo");
        floatInfo.transform.localPosition = Vector3.zero;
        floatInfo.Init(nameKey);
    }
    private void OnTriggerEnter(Collider other)
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsServer) return;
#endif
        if (PlayerManager.Instance != null && other.gameObject == PlayerManager.Instance.localPlayer.gameObject && prompt != null)
        {
            prompt.SetActive(true);
        }

    }
    /// <summary>
    /// 触发时保持提示出现
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsServer) return;
#endif
        if (PlayerManager.Instance != null && other.gameObject == PlayerManager.Instance.localPlayer.gameObject)
        {
            if (prompt != null && Camera.main != null)
            {
                //提示感叹号一直朝向玩家
                prompt.transform.LookAt(Camera.main.transform);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                OnPlayerInteraction();


            }
        }
    }
    protected virtual void OnPlayerInteraction()
    {

    }
    private void OnTriggerExit(Collider other)
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsServer) return;
#endif
        if (other.gameObject == PlayerManager.Instance.localPlayer.gameObject && prompt != null)
        {
            prompt.SetActive(false);
        }

    }
}
