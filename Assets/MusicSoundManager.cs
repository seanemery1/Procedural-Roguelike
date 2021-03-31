using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSoundManager : MonoBehaviour
{
    public static AudioClip dungeonMusic;
    public static AudioSource audioSrc;
    void Start()
    {
        dungeonMusic = Resources.Load<AudioClip>("Sounds/Music/DungeonMusic");

        audioSrc = GetComponent<AudioSource>();
        
        audioSrc.clip = dungeonMusic;
        audioSrc.loop = true;
        audioSrc.Play();

    }
}
