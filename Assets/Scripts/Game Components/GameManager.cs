using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// A generic enum class that is used to identify the game's current state (a state machine)
public enum GameState
{
    onGoing,
    gameOver,
    gameVictory,
    pause
}

// Game manager class to identify whether a game is paused/unpaused or if the player had achieved either victory (by killing the final boss) or were defeated (and so they get a game over screen)
public class GameManager : MonoBehaviour
{
    // Initializing variables by grabbing UI components for each screen
    float transitionTime = 3f;
    public GameObject UI;
    public GameObject GameOverScreen;
    public GameObject GameVictoryScreen;
    public GameObject PauseMenu;
    public GameObject tileMap;
    public TMP_Text seedInfo;
    GameState gameState;
  
    // Getting seed info to display on pause menu and setting the game's state to onGoing (aka unpaused)
    private void Start()
    {
        Physics2D.IgnoreLayerCollision(9, 10, false);
        seedInfo.text = "Seed: " + tileMap.GetComponent<Seed>().CurrentSeed;
        gameState = GameState.onGoing;
    }

    public void EndGame(bool win)
    {
        if (gameState.Equals(GameState.onGoing))
        {
            if (win)
            {
                FindObjectOfType<MusicSoundManager>().ChangeMusic("victoryMusic");
                gameState = GameState.gameVictory;
                //Time.timeScale = 0;
                Invoke("GameVictory", transitionTime / 3f);
            }
            else
            {
                FindObjectOfType<MusicSoundManager>().ChangeMusic("gameOver");
                gameState = GameState.gameOver;

                Invoke("GameOver", transitionTime);
            }
        }

    }
    void GameOver()
    {
        Time.timeScale = 0;
        UI.SetActive(false);
        GameOverScreen.SetActive(true);
        Physics2D.IgnoreLayerCollision(9, 10, false);
        Debug.Log("gameover");
    }
    void GameVictory()
    {
        Time.timeScale = 0;
        UI.SetActive(false);
        GameVictoryScreen.SetActive(true);
        Physics2D.IgnoreLayerCollision(9, 10, false);
        Debug.Log("victory");
    }
    public void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Debug.Log("Down");
            if (gameState.Equals(GameState.onGoing) || gameState.Equals(GameState.pause))
            {
                Pause();
            }
        }
    }
    public void Pause()
    {
        if (gameState.Equals(GameState.onGoing))
        {
            gameState = GameState.pause;
            Time.timeScale = 0;
            UI.SetActive(false);
            PauseMenu.SetActive(true);
        } else if (gameState.Equals(GameState.pause))
        {
            gameState = GameState.onGoing;
            Time.timeScale = 1f;
            PauseMenu.SetActive(false);
            UI.SetActive(true);
        }

    }
    public void Quit() {
        Physics2D.IgnoreLayerCollision(9, 10, false);
        MusicSoundManager.audioSrc.Stop();
        Time.timeScale = 1;
        SceneManager.LoadScene("Menu");
    }
    public void NewLevel()
    {
        Physics2D.IgnoreLayerCollision(9, 10, false);
        Time.timeScale = 1;
        //.GameSeed = "Default";
        tileMap.GetComponent<Seed>().GameSeed = "Default";
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
