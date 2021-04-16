using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sound Manager for short sound effects (everything but the music)
public class SoundManager : MonoBehaviour
{
    // Variables to store all the sound effects that will be used.
    public static AudioClip playerAttack, playerHit, playerWalk, playerDeath, doorOpen, doorFail, skeletonHit, skeletonDeath, miscCollect;
    public static AudioSource audioSrc;
    public static float volume;

    // Initialize volume level on Scene load (Menu -> Game).
    private void Awake()
    {
        try
        {
            volume = MainMenu.volume;
        }
        catch
        {
            volume = 1f;
        }
    }

    // Initializing all the sound effects before the first frame is called.
    void Start()
    {
        playerAttack = Resources.Load<AudioClip>("Sounds/Player/PlayerAttack");
        playerHit = Resources.Load<AudioClip>("Sounds/Player/PlayerHit");
        playerWalk = Resources.Load<AudioClip>("Sounds/Player/PlayerWalk");
        playerDeath = Resources.Load<AudioClip>("Sounds/Player/PlayerDeath");
        doorOpen = Resources.Load<AudioClip>("Sounds/Door/DoorOpen");
        doorFail = Resources.Load<AudioClip>("Sounds/Door/DoorFail");
        skeletonHit = Resources.Load<AudioClip>("Sounds/Skeleton/SkeletonHit");
        skeletonDeath = Resources.Load<AudioClip>("Sounds/Skeleton/SkeletonDeath");
        miscCollect = Resources.Load<AudioClip>("Sounds/Misc/Collect");

        audioSrc = GetComponent<AudioSource>();
    }

    // Switch case method to play corresponding sound effects when called upon from other classes.
    public static void PlaySound(string clip)
    {
        switch (clip)
        {
            case "playerAttack":
                audioSrc.PlayOneShot(playerAttack, volume);
                break;
            case "playerHit":
                audioSrc.PlayOneShot(playerHit, volume);
                break;
            case "playerWalk":
                audioSrc.PlayOneShot(playerWalk, 0.25f*volume);
                break;
            case "playerDeath":
                audioSrc.PlayOneShot(playerDeath, volume);
                break;
            case "doorOpen":
                audioSrc.PlayOneShot(doorOpen, volume);
                break;
            case "doorFail":
                audioSrc.PlayOneShot(doorFail, volume);
                break;
            case "skeletonHit":
                audioSrc.PlayOneShot(skeletonHit, volume);
                break;
            case "skeletonDeath":
                audioSrc.PlayOneShot(skeletonDeath, volume);
                break;
            case "miscCollect":
                audioSrc.PlayOneShot(miscCollect, volume);
                break;
        }
    }
   
}
