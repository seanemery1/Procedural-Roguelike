using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float thrust;
    public float knockTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
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
    }
}
