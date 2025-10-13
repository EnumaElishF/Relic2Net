using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NPCControllerBase : MonoBehaviour
{
    [SerializeField] protected string configName;
    //[SerializeField] protected DialogConfig defaultDialogConfig;
    //[SerializeField] protected string taskID;
    [SerializeField] protected GameObject prompt;
    //[SerializeField] protected Transform floatInfoPoint;
    public abstract string nameKey { get; }

    private void Start()
    {
        if (prompt != null) prompt.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
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
        if (other.gameObject == PlayerManager.Instance.localPlayer.gameObject && prompt != null)
        {
            prompt.SetActive(false);
        }

    }
}
