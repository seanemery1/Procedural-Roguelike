using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// An generic enum class that is used to identify the enemies states (a state machine)
public enum EnemyState
{
    walk,
    attack,
    idle,
    stunned
}

// An inheritable class that contains most of the generic variables for different types of enemies.
// Most of the methods from SkeletonAI/SkeletraxAi should be moved to this class (to make scaling/adding more enemies easier).
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
}
