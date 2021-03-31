using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static AudioClip playerAttack, playerHit, playerWalk, doorOpen, doorFail, skeletonHit, skeletonDeath, miscCollect;
    public static AudioSource audioSrc;

    void Start()
    {
        playerAttack = Resources.Load<AudioClip>("Sounds/Player/PlayerAttack");
        playerHit = Resources.Load<AudioClip>("Sounds/Player/PlayerHit");
        playerWalk = Resources.Load<AudioClip>("Sounds/Player/PlayerWalk");
        doorOpen = Resources.Load<AudioClip>("Sounds/Door/DoorOpen");
        doorFail = Resources.Load<AudioClip>("Sounds/Door/DoorFail");
        skeletonHit = Resources.Load<AudioClip>("Sounds/Skeleton/SkeletonHit");
        skeletonDeath = Resources.Load<AudioClip>("Sounds/Skeleton/SkeletonDeath");
        miscCollect = Resources.Load<AudioClip>("Sounds/Misc/Collect");

        audioSrc = GetComponent<AudioSource>();
    }

    public static void PlaySound(string clip)
    {
        switch (clip)
        {
            case "playerAttack":
                audioSrc.PlayOneShot(playerAttack);
                break;
            case "playerHit":
                audioSrc.PlayOneShot(playerHit);
                break;
            case "playerWalk":
                audioSrc.PlayOneShot(playerWalk, 0.25f);
                break;
            case "doorOpen":
                audioSrc.PlayOneShot(doorOpen);
                break;
            case "doorFail":
                audioSrc.PlayOneShot(doorFail);
                break;
            case "skeletonHit":
                audioSrc.PlayOneShot(skeletonHit);
                break;
            case "skeletonDeath":
                audioSrc.PlayOneShot(skeletonDeath);
                break;
            case "miscCollect":
                audioSrc.PlayOneShot(miscCollect);
                break;
        }
    }
   
}
