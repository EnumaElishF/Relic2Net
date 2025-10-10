
#if !UNITY_SERVER||UNITY_EDITOR
using UnityEngine;

public class MerchantController : MonoBehaviour
{
    [SerializeField] private string merchantConfigName;
    [Header("提示")]
    [SerializeField] private GameObject prompt;
    private void Start()
    {
        if(prompt !=null) prompt.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(PlayerManager.Instance != null && other.gameObject == PlayerManager.Instance.localPlayer.gameObject && prompt != null)
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
        if(PlayerManager.Instance != null && other.gameObject == PlayerManager.Instance.localPlayer.gameObject)
        {
            if (prompt != null && Camera.main!=null )
            {
                //提示感叹号一直朝向玩家
                prompt.transform.LookAt(Camera.main.transform);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                //交互
                PlayerManager.Instance.OpenShop(merchantConfigName);

            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerManager.Instance.localPlayer.gameObject && prompt != null)
        {
            prompt.SetActive(false);
        }

    }
}
#endif
