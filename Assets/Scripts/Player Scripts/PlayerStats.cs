using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public int health = 6;
    public bool dmgImmunity = false;
    public int keys = 6;
    // Start is called before the first frame update
    public bool useKeyOnDoor()
    {
        if (keys>0)
        {
            keys--;
            return true;
        } else
        {
            return false;
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
