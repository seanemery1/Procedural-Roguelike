using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Keeps track of the player's stats and what information should be displayed on the HUD/UI
public class PlayerStats : MonoBehaviour
{
    // Variables declaration/initialization
    [Header("Player Stats")]
    public int health = 6;
    [SerializeField] private int numOfHearts = 6;

    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;
    public TMP_Text numsOfKeysText; 
    public bool dmgImmunity = false;

    [SerializeField] private Image key;
    [SerializeField] private Sprite keySprite;
    public int numsOfKeys;
    [SerializeField] private GameObject UI;
    
    // Initialize UI key element to 0
    public void Start()
    {
        numsOfKeys = 0;
        numsOfKeysText.text = "x" + numsOfKeys.ToString();
    }

    // Detects when player has collided with either a key or health, plays corresponding sound effects and increases player's stats
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Key"))
        {
            Debug.Log("Key Hit");
            numsOfKeys++;
            numsOfKeysText.text = "x" + numsOfKeys.ToString();
            SoundManager.PlaySound("miscCollect");
            other.gameObject.SetActive(false);
        }
        if (other.collider.CompareTag("Health"))
        {
            Debug.Log("Health Hit");
            health += 2;
            SoundManager.PlaySound("miscCollect");
            other.gameObject.SetActive(false);
        }
    }

    // Used to decrement numsOfKey if a door was successfully opened
    public bool useKeyOnDoor()
    {
        if (numsOfKeys>0)
        {
            SoundManager.PlaySound("doorOpen");
            
            numsOfKeys--;
            numsOfKeysText.text = "x" + numsOfKeys.ToString();
            return true;
        } else
        {
            SoundManager.PlaySound("doorClose");
            return false;
        }
    }
  
    // Updates the player's stats for the HUD/UI every frame
    void Update()
    {
        // If a player collects more hearts than the maximimum number of hearts, then make health = max hearts
        if (health>numOfHearts)
        {
            health = numOfHearts;
        }
        // Using a Div 2 function to determine whether the UI should display a half heart (1 health) or a full heart (2 health)
        for (int i = 0; i < hearts.Length*2; i = i + 2)
        {
            if (i < health)
            {
                    hearts[i / 2].sprite = fullHeart;
                
            } else
            {
                hearts[i / 2].sprite = emptyHeart;
            }
            if (health%2==1)
            {
                hearts[health / 2].sprite = halfHeart;
            }
            if (i/2 < numOfHearts/2)
            {
                hearts[i / 2].enabled = true;
            } else
            {
                hearts[i / 2].enabled = false;
            }
        }
        
    }
}
