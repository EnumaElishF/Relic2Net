using JKFrame;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Config/ClientGlobalConfig")]
public class ClientGlobalConfig : ConfigBase
{
    public AudioClip[] playerFootStepAudios;
    public float playerMaxHp;

    public List<AudioClip[]> monsterFootStepAudioList;
}
