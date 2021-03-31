using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public int health = 6;
    public int numOfHearts = 6;

    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite halfHeart;
    public Sprite emptyHeart;
    public TMP_Text numsOfKeysText; 
    public bool dmgImmunity = false;

    public Image key;
    public Sprite keySprite;
    public int numsOfKeys;
    public GameObject UI;
    // Start is called before the first frame update
    public void Start()
    {
        numsOfKeys = 0;
        numsOfKeysText.text = "x" + numsOfKeys.ToString();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Key Hit");
        if (other.collider.CompareTag("Key"))
        {
            Debug.Log("Key Hit1");
            numsOfKeys++;
            numsOfKeysText.text = "x" + numsOfKeys.ToString();
            SoundManager.PlaySound("miscCollect");
            other.gameObject.SetActive(false);
        }
    }
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
  

    void Update()
    {
        if (health>numOfHearts)
        {
            health = numOfHearts;
        }
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
