using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Seed : MonoBehaviour
{
    public string GameSeed;
    public int CurrentSeed = 0;
    //

    private void Awake() 
    {
       try
        {
            GameSeed = MainMenu.seed;
            MainMenu.seed = "Default";
           
        } catch
        {
            GameSeed = "Default";
        }
       int number;
        bool success = int.TryParse(GameSeed, out number);
            if (success)
            {
                CurrentSeed = number;
                UnityEngine.Random.InitState(number);
            }
            else if (GameSeed.Equals("Default"))
            {
                CurrentSeed = DateTime.Now.Ticks.GetHashCode();
                UnityEngine.Random.InitState(CurrentSeed);
            }
            else
            {
                CurrentSeed = GameSeed.GetHashCode();
                UnityEngine.Random.InitState(CurrentSeed);
            }
    }
}
