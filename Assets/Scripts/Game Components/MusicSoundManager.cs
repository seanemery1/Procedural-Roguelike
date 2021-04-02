using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSoundManager : MonoBehaviour
{
    public static AudioClip dungeonMusic, gameOver, bossMusic, menuMusic, victoryMusic;
    public static AudioSource audioSrc;
    public static float secondsToFadeOut = 1f;
    public float maxVolume;

    private void Awake()
    {
        try
        {
            maxVolume = MainMenu.volume;
            //MainMenu menu = GameObject.FindGameObjectWithTag("Settings").GetComponent<MainMenu>();
            //maxVolume = menu.volume;

        }
        catch
        {
            maxVolume = 1f;
        }
    }
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
    IEnumerator FadeMusicTransition(AudioClip music, bool instant)
    {
        

        // Check Music Volume and Fade Out
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

        // Make sure volume is set to 0
        audioSrc.volume = 0;

        // Stop Music
        audioSrc.Stop();

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
