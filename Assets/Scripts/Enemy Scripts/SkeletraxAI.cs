using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Inherits methods and variables from the Enemy class
public class SkeletraxAI : Enemy
{
    // Extra variables used to detect if a player enter the same room where the boss resides.
    public BoxCollider2D boxCollider;
    public RectInt room;
   
    // Initializing all the variables before the first frame is called.
    void Start()
    {
        boxCollider = GetComponentInChildren<BoxCollider2D>();
        boxCollider.size = room.size;
        health = 20;
        currentState = EnemyState.idle;
    }

    // Update method is called every frame and it runs the appropriate methods depending on the enemy's current state.
    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.idle:
                Idle();
                break;
        }
    }

    // The boss currently has no AI and therefore doesn't do anything during idle.
    private void Idle()
    {

    }

    // If player enters the boss room trigger range, change music to boss music.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FindObjectOfType<MusicSoundManager>().ChangeMusic("bossMusic");
        }
    }

    //// If hit by player's attack, call GetHit() method
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.collider.CompareTag("PlayerAttack"))
    //    {
    //        GetHit();
    //    }
    //}

    // Method to called by a player's class when the enemy gets hit by a player. Reduces enemy health by 1 and plays appropriate damage taken/death animation and sound effects.
    private IEnumerator FlashCo(bool death)
    {
        int temp = 0;
        // Alternating between red flashing and original sprite
        while (temp < numberOfFlashes)
        {
            mySprite.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            mySprite.color = regularColor;
            yield return new WaitForSeconds(flashDuration);
            temp++;
        }
        // If dead, remove self from the game.
        if (death)
        {
            this.gameObject.SetActive(false);
        }

    }

    // Method to called by a player's class when the enemy gets hit by a player. Reduces enemy health by 1 and plays appropriate damage taken/death animation and sound effects.
    new public void GetHit()
    {
        Debug.Log("Health: " + health);
        health -= 1;
        if (health > 0)
        {
            SoundManager.PlaySound("skeletonHit");
            StartCoroutine(FlashCo(false));
        }
        else
        {
            // If Skeletrax (the boss) dies, change state of the game to "End" with a player victory boolean.
            SoundManager.PlaySound("skeletonDeath");
            StartCoroutine(FlashCo(true));
            FindObjectOfType<GameManager>().EndGame(true);

        }
    }
}
