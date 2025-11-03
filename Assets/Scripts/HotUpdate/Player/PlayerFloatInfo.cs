using UnityEngine;

public class PlayerFloatInfo : MonoBehaviour
{
    [SerializeField] private TextMesh nameText;
    [SerializeField] private SpriteRenderer hpBarFillSpriteRenderer;
    public void UpdateName(string name)
    {
        nameText.text = name;
    }
    public void UpdateHp(float fillAmount)
    {
        //hp在3D模式下的图已经被设计成了，按照Scale从0到1的缩放就能控制0到100%的血量控制变动关系
        hpBarFillSpriteRenderer.transform.localScale = new Vector3(fillAmount, 1, 1);
    }
    void LateUpdate()
    {
        //玩家头顶信息PlayerFloatInfo，始终朝向摄像机的方向
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform.position);
        }
    }
}
