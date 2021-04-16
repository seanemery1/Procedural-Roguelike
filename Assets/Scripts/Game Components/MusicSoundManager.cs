using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sound Manager for music only
public class MusicSoundManager : MonoBehaviour
{
    // Variables to store all the sound effects that will be used.
    public static AudioClip dungeonMusic, gameOver, bossMusic, menuMusic, victoryMusic;
    public static AudioSource audioSrc;
    public static float secondsToFadeOut = 1f;
    public float maxVolume;

    // Initialize volume level on Scene load (Menu -> Game).
    private void Awake()
    {
        try
        {
            maxVolume = MainMenu.volume;
        }
        catch
        {
            maxVolume = 1f;
        }
    }

    // Initializing all the music before the first frame is called.
    void Start()
    {
        dungeonMusic = Resources.Load<AudioClip>("Sounds/Music/DungeonMusic");
        gameOver = Resources.Load<AudioClip>("Sounds/Music/GameOver");
        victoryMusic = Resources.Load<AudioClip>("Sounds/Music/VictoryMusic");
        menuMusic = Resources.Load<AudioClip>("Sounds/Music/MenuMusic");
        bossMusic = Resources.Load<AudioClip>("Sounds/Music/BossMusic");
        audioSrc = GetComponent<AudioSource>();
        

        audioSrc.clip = dungeonMusic;
        audioSrc.volume = maxVolume;
        audioSrc.loop = true;
        audioSrc.Play();

    }

    // Switch case method to play the right music when called upon from other classes.
    public void ChangeMusic(string music)
    {
        switch (music)
        {
            case "dungeonMusic":
                StartCoroutine(FadeMusicTransition(dungeonMusic, false));
                break;
            case "gameOver":
                StartCoroutine(FadeMusicTransition(gameOver, false));
                break;
            case "menuMusic":
                StartCoroutine(FadeMusicTransition(menuMusic, true));
                break;
            case "victoryMusic":
                StartCoroutine(FadeMusicTransition(victoryMusic, false));
                break;
            case "bossMusic":
                StartCoroutine(FadeMusicTransition(bossMusic, false));
                break;
          
        }
    }

    // Async method to fade out old music and fade in the new music across multiple frames.
    IEnumerator FadeMusicTransition(AudioClip music, bool instant)
    {
        // Checks music volume and fades volume to 0 either gradually or instantly (depending on instant boolean).
        while (audioSrc.volume > 0.01f)
        {
            if (instant)
            {
                audioSrc.volume = 0;
            } else
            {
                audioSrc.volume -= Time.unscaledDeltaTime / (maxVolume * secondsToFadeOut);
            }
            yield return null;
        }

        // Set volume to 0 (in the event that 0 < volume < 0.01f).
        audioSrc.volume = 0;

        // Stop current track.
        audioSrc.Stop();

        // Change to new track and make it loop, play then slowly fade in (independent of physics status with unscaledDeltaTime).
        audioSrc.clip = music;
        audioSrc.loop = true;
        audioSrc.Play();
        while (audioSrc.volume < maxVolume)
        {
            audioSrc.volume += Time.unscaledDeltaTime / (maxVolume * secondsToFadeOut);
            yield return null;
        }
    }
}
