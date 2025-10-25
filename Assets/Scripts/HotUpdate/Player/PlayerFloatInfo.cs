using UnityEngine;

public class PlayerFloatInfo : MonoBehaviour
{
    [SerializeField] private TextMesh nameText;
    public void Init(string name)
    {
        nameText.text = name;
    }
    private void Update()
    {
        //玩家头顶信息PlayerFloatInfo，始终朝向摄像机的方向
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform.position);
        }
    }
}
