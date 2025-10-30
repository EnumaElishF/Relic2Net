using JKFrame;
using UnityEngine;
/// <summary>
/// 场景音效
/// </summary>
public class TerrainAudioController : MonoBehaviour
{
    private AudioSource[] audioSources;
    void Start()
    {
#if UNITY_EDITOR
        if (NetManager.Instance.IsServer)
        {
            Destroy(this);
            return;
        }
#endif
        audioSources = GetComponentsInChildren<AudioSource>();
        EventSystem.AddTypeEventListener<MusicVolumeChangedEvent>(OnMusicVolumeChangedEvent);
        OnMusicVolumeChangedEvent(default);
    }

    private void OnDestroy()
    {
        EventSystem.RemoveTypeEventListener<MusicVolumeChangedEvent>(OnMusicVolumeChangedEvent);
    }

    private void OnMusicVolumeChangedEvent(MusicVolumeChangedEvent arg)
    {
        foreach (var item in audioSources)
        {
            item.volume = AudioSystem.BGVolume;
        }
    }
}
