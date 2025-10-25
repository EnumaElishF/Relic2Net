using UnityEngine;

// 允许脚本在编辑模式下运行，方便实时检查
public class NPCCheckNull : MonoBehaviour
{
    // 当组件被加载或属性被修改时触发检查
    private void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            CheckSkinnedMeshMaterials();
        }
    }

    /// <summary>
    /// 检查当前物体上的SkinnedMeshRenderer组件，验证Materials前3个元素是否为空
    /// </summary>
    public void CheckSkinnedMeshMaterials()
    {
        // 获取当前物体上的SkinnedMeshRenderer组件
        SkinnedMeshRenderer skinnedRenderer = GetComponent<SkinnedMeshRenderer>();

        // 情况1：没有SkinnedMeshRenderer组件
        if (skinnedRenderer == null)
        {
            Debug.LogWarning($"GameObject [{gameObject.name}] 上没有挂载 SkinnedMeshRenderer 组件！", this);
            return;
        }

        // 获取材质数组（注意：Materials是复制数组，修改不会影响原组件，这里仅用于检查）
        Material[] materials = skinnedRenderer.materials;

        // 检查前3个元素（索引0、1、2）
        for (int i = 0; i < 3; i++)
        {
            // 情况2：数组长度不足，当前索引超出范围（例如数组只有2个元素，索引2不存在）
            if (i >= materials.Length)
            {
                Debug.LogWarning($"SkinnedMeshRenderer 的 Materials 数组长度不足！索引 [{i}] 不存在（当前长度：{materials.Length}）", this);
            }
            // 情况3：索引存在但材质为空
            else if (materials[i] == null)
            {
                Debug.LogWarning($"SkinnedMeshRenderer 的 Materials 索引 [{i}] 材质为空！", this);
            }
        }

        // 所有检查通过时的提示（可选）
        if (materials.Length >= 3 && materials[0] != null && materials[1] != null && materials[2] != null)
        {
            Debug.Log($"GameObject [{gameObject.name}] 的 SkinnedMeshRenderer 前3个材质均正常", this);
        }
    }
}