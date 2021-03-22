using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Seed : MonoBehaviour
{
    public string GameSeed = "Default";
    public int CurrentSeed = 0;

    private void Awake()
    {
        if (GameSeed.Equals("Default"))
        {
            CurrentSeed = DateTime.Now.Ticks.GetHashCode();
            UnityEngine.Random.InitState(CurrentSeed);
        } else
        {
            CurrentSeed = GameSeed.GetHashCode();
            UnityEngine.Random.InitState(CurrentSeed);
        }
        
    }
}
