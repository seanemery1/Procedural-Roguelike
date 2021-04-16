using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A generic enum class that is used to identify the enemy's current state (a state machine)
public enum EnemyState
{
    walk,
    attack,
    idle,
    stunned
}

// An inheritable class that contains most of the generic variables for different types of enemies
// Most of the methods from SkeletonAI/SkeletraxAi should be moved to this class (to make scaling/adding more enemies easier)
public class Enemy : MonoBehaviour
{
    public EnemyState currentState;

    public float waitTime;
    public float startWaitTime;
    public float decisionWaitTime;
    public float startDecisionWaitTime;
    public float moveSpeed;
    public Vector2 moveDirection;
    public int health;
    public string enemyName;
    public int baseAttack;
    public Rigidbody2D rb;

    public bool isIdle;
    public bool isWalk;
    public bool isAttack;
    public bool isStunned;

    [Header("IFrame Stuff")]
    public Color flashColor;
    public Color regularColor;
    public float flashDuration;
    public int numberOfFlashes;
    public SpriteRenderer mySprite;

  
    // Inheritable methods that aren't found in MonoBehavior
    private void Idle()
    {

    }

    void Attack()
    {
        
    }

    void Walk()
    {
       
    }

    void Stunned()
    {
       
    }

    public void GetHit()
    {
      
    }
}
