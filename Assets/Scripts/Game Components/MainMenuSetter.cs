using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class MainMenuSetter : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_InputField inputField;
    public AudioSource audioSrc;
    private void Start()
    {
        MainMenu.seed = "Default";
        audioSrc.volume = MainMenu.volume;
        volumeSlider.value = MainMenu.volume;
    }
    public void UpdateSeed()
    {
        MainMenu.seed = inputField.text;
        if (MainMenu.seed.Equals(""))
        {
            MainMenu.seed = "Default";
        }
    }
    public void UpdateVolume()
    {
        MainMenu.volume = volumeSlider.value;
        audioSrc.volume = volumeSlider.value;
    }
    public void PlayGame()
    {
        //audioSrc.volume = 0;
        audioSrc.Stop();
        SceneManager.LoadScene("Game");
    }
    public static void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
