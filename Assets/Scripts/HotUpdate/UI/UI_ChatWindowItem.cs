using UnityEngine;
using UnityEngine.UI;

public class UI_ChatWindowItem : MonoBehaviour
{
    [SerializeField] private Text text;
    public void Init(string name, string content)
    {
        text.text = $"<color=yellow>{name}</color> : {content}";
    }
}
