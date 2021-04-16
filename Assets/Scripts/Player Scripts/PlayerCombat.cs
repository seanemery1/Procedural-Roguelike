using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls player attack animations and constraints
public class PlayerCombat : MonoBehaviour
{
    // Variables for animations and hit detection (hit detection using this class is deprecated)
    public bool combatEnabled;
    [SerializeField]
    private float inputTimer, attackRadius, attackDamage;
    [SerializeField]
    private Transform attackHitBoxPos;
    [SerializeField]
    private LayerMask whatIsDamageable;
    private bool gotInput, isAttacking;

    private float lastInputTime = Mathf.NegativeInfinity;

    public Animator animator;

    // Attack inputs and attack animation handling is done every frame
    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    // Initialize variables before first frame
    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("canAttack", combatEnabled);
    }

    // Checks if attack button is pressed
    private void CheckCombatInput()
    {
        if (Input.GetButtonDown("Fire3")) // Y button on Xbox controller, J on keyboard
        {
            Debug.unityLogger.Log("Swing Attack");
            if (combatEnabled)
            {
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    // Checks if attack input is received
    private void CheckAttacks()
    {
        if (gotInput)
        {
            // If Attack button is pressed and player is not currently attacking, set attack animation to true and play attack sound
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                animator.SetBool("isAttacking", isAttacking);
                SoundManager.PlaySound("playerAttack");

            }
            //Perform attack
        }
        // If attack duration is over, set gotInput to false
        if (Time.time >= lastInputTime + inputTimer)
        {
            gotInput = false;
        }
    }

    // Deprecated attack collision detection
    private void CheckAttackHitBox()
    {
        // Initialize collision array
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attackHitBoxPos.position, attackRadius, whatIsDamageable);
        
        // For each object that is within the collision radius, inform that object that it has been damaged
        foreach (Collider2D collider in detectedObjects)
        {
            collider.transform.parent.SendMessage("Damage", attackDamage);
        }
    }

    // Function called by an Attack animation event to disable the attack animations when the attack finishes
    private void FinishAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", isAttacking);
    }

    // Enables debug view of attack radius in editor
     void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(attackHitBoxPos.position, attackRadius);
    }
}
