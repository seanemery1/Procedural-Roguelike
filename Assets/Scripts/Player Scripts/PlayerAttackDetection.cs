using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Determines what object/enemy the player has hit and what to do in each case
public class PlayerAttackDetection : MonoBehaviour
{
    // Kockback variables (for velocity)
    [SerializeField] private float thrust;
    [SerializeField] private float knockTime;

    // When Player's attack trigger hits a collider, check for enemy tag. 
    private void OnTriggerEnter2D(Collider2D other)
    {
        // If enemy is detected, set enemy's direction/movement direction to simulate knock back. Set enemy's state to "Stunned" and calls their GetHit() method.
        if (other.gameObject.CompareTag("Enemy"))
        {
            Rigidbody2D enemy = other.GetComponent<Rigidbody2D>();
            if (enemy != null)
            {
                Vector2 difference = enemy.transform.position - transform.position;
                difference = difference.normalized * thrust;
                enemy.GetComponent<Enemy>().moveDirection = difference;
                enemy.GetComponent<Enemy>().isStunned = true;
                enemy.GetComponent<Enemy>().currentState = EnemyState.stunned;
                if (enemy.GetComponent<SkeletonAI>()!=null)
                {
                    enemy.GetComponent<SkeletonAI>().GetHit();
                }
            }
        }
        // If Skeletrax (the boss) is detected, call their GetHit() method (Skeletrax is currently immune to knockback, might change in the future).
        if (other.gameObject.CompareTag("Skeletrax"))
        {
            Rigidbody2D enemy = other.GetComponent<Rigidbody2D>();
            if (enemy != null)
            {
                if (enemy.GetComponent<SkeletraxAI>() != null)
                {
                    enemy.GetComponent<SkeletraxAI>().GetHit();
                }
            }
        }
    }
}
