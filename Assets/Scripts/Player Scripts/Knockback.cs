using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    // Start is called before the first frame update
    public float thrust;
    public float knockTime;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Rigidbody2D enemy = other.GetComponent<Rigidbody2D>();
            if (enemy != null)
            {
                //enemy.isKinematic = false;
                //enemy.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                //enemy.GetComponent<Enemy>().isStunned = true;
               
                Vector2 difference = enemy.transform.position - transform.position;
                difference = difference.normalized * thrust;
                enemy.GetComponent<Enemy>().moveDirection = difference;
                enemy.GetComponent<Enemy>().isStunned = true;
                enemy.GetComponent<Enemy>().currentState = EnemyState.stunned;
                if (enemy.GetComponent<SkeletonAI>()!=null)
                {
                    enemy.GetComponent<SkeletonAI>().GetHit();
                }
                
                //StartCoroutine(KnockCo(enemy));

            }
        }
    }
    //private IEnumerator KnockCo(Rigidbody2D enemy)
    //{
        //if (enemy!=null)
        //{
        //    yield return new WaitForSeconds(knockTime);
        //    enemy.velocity = Vector2.zero;
        //    enemy.GetComponent<Enemy>().isStunned = false;
        //    //enemy.transform.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        //    //enemy.isKinematic = true;
        //}
    //}
}
