using UnityEngine;

public class NPCFloatInfo : MonoBehaviour
{
    [SerializeField] private TextMesh nameText;
    private string nameKey;
    public void Init(string nameKey)
    {
        this.nameKey = nameKey;
        UpdateName(LocalizationSystem.LanguageType);
        LocalizationSystem.RegisterLanguageEvent(UpdateName);
    }
    private void OnDisable()
    {
        LocalizationSystem.UnregisterLanguageEvent(UpdateName);
    }
    private void UpdateName (LanguageType language)
    {
        nameText.text = LocalizationSystem.GetContent<LocalizationStringData>(nameKey, language).content;
    }
    private void Update()
    {
        //FloatInfo，始终朝向摄像机的方向
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform.position);
        }
    }
}
