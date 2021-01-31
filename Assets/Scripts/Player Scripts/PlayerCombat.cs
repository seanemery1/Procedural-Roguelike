using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField]
    private bool combatEnabled;
    [SerializeField]
    private float inputTimer, attackRadius, attackDamage;
    [SerializeField]
    private Transform attackHitBoxPos;
    [SerializeField]
    private LayerMask whatIsDamageable;
    private bool gotInput, isAttacking;

    private float lastInputTime = Mathf.NegativeInfinity;

    public Animator animator;
    private void Update()
    {
        CheckCombatInput();
        CheckAttacks();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("canAttack", combatEnabled);
    }
    private void CheckCombatInput()
    {
        if (Input.GetButtonDown("Fire3")) // Y button
        {
            Debug.unityLogger.Log("Fire3");
            if (combatEnabled)
            {
                // Attempt combat
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void CheckAttacks()
    {
        if (gotInput)
        {
            if (!isAttacking)
            {
                gotInput = false;
                isAttacking = true;
                animator.SetBool("isAttacking", isAttacking);

            }
            //Perform attack
        }

        if (Time.time >= lastInputTime + inputTimer)
        {
            gotInput = false;
        }
    }

    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attackHitBoxPos.position, attackRadius, whatIsDamageable);

        foreach (Collider2D collider in detectedObjects)
        {
            collider.transform.parent.SendMessage("Damage", attackDamage);
            //Instantiate hit particle
        }
    }
    private void FinishAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", isAttacking);
    }
     void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(attackHitBoxPos.position, attackRadius);
        Debug.Log("I am working");
    }
}
