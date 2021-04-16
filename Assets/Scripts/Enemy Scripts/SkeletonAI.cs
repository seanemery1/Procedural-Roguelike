using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A class that dictates the enemy's AI (structured as a state machine)
public class SkeletonAI : Enemy
{

    // Extra variables needed specifically by the Skeleton class
    public Animator animator;
    public Transform target;
    public PolygonCollider2D playerTrigger;
    public GameObject heart;
    public float horizontal;
    public float vertical;
    public Vector2 playerPosition;
    private int wallBump;


    // Initializing all the variables before the first frame is called.
    void Start()
    {
        isStunned = false;
        isAttack = false;
        isWalk = false;
        isIdle = false;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        //health = 2;
        startWaitTime = 0.5f;
        moveSpeed = 150;
        waitTime = startWaitTime;
        moveDirection = new Vector2(0, 0);
        animator.SetBool("isWalking", false);
        flashDuration = 0.08f;
        numberOfFlashes = 3;
        currentState = EnemyState.idle;
        mySprite = GetComponent<SpriteRenderer>();  
    }

    // Update method is called every frame and it runs the appropriate methods depending on the enemy's current state.
    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.walk:
                Walk();
                break;
            case EnemyState.idle:
                Idle();
                break;
            case EnemyState.attack:
                Attack();
                break;
            case EnemyState.stunned:
                Stunned();
                break;
            default:
                Debug.Log("do nothing");
                break;

        }
    }
    // Fixed Update method is used for physics calculation (walking/velocities) as time is constant (whereas the deltaTime on Update() is dependent on a variable framerate).
    private void FixedUpdate()
    {
        if (currentState.Equals(EnemyState.walk) || currentState.Equals(EnemyState.attack) || currentState.Equals(EnemyState.stunned))
        {
            moveDirection = moveDirection.normalized;
            rb.velocity = new Vector2((Mathf.Round((moveDirection.x * moveSpeed * Time.deltaTime) / 0.0625f) * 0.0625f), (Mathf.Round((moveDirection.y * moveSpeed * Time.deltaTime) / 0.0625f) * 0.0625f));
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }
    }
    // Check if player runs over the enemy's field of view trigger.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // While enemy is not stunned, check if player is within FOV trigger.
        if (!currentState.Equals(EnemyState.stunned))
        {
            // If player is in FOV (and the skeleton is not stunned), track the player's position and "attack" the player.
            if (other.gameObject.CompareTag("Player"))

            {

                isAttack = true;
                moveDirection = new Vector2(other.transform.position.x - transform.position.x, other.transform.position.y - transform.position.y);

                currentState = EnemyState.attack;
            }
        }
        else
        {
            isAttack = false;
        }

    }
    // Checks if player is still within the enemy's FOV.
    public void OnTriggerStay2D(Collider2D other)
    {
        // While enemy is not stunned, check if player is within FOV trigger.
        if (!currentState.Equals(EnemyState.stunned))
        {
            // If player is in FOV (and the skeleton is not stunned), track the player's position and "attack" the player.
            if (other.gameObject.CompareTag("Player"))
            {
                isAttack = true;
                moveDirection = new Vector2(other.transform.position.x - transform.position.x, other.transform.position.y - transform.position.y);
                currentState = EnemyState.attack;
            }
        }
        else
        {
            isAttack = false;
        }

    }
    // Checks if player has left the enemy's FOV.
    private void OnTriggerExit2D(Collider2D other)
    {
        //if (other.tag == "Player")
        if (other.gameObject.CompareTag("Player"))
        {
            isAttack = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Bump" + collision.collider.tag);
        if (collision.collider.CompareTag("Wall"))
        {
            
            wallBump++;
            Debug.Log("wall" + wallBump);
            isWalk = false;
            isAttack = false;
            moveSpeed = 150f;
            waitTime = 1f;
            animator.SetBool("isWalking", false);
            currentState = EnemyState.idle;
        }
    }

    // If stunned, stop all animations and reduce the object's velocity to 0 for a specified delay.
    void Stunned()
    {
        if (isStunned)
        {           
            waitTime = 0.4f;
            animator.SetBool("isWalking", false);
            moveSpeed = 150f;
            isWalk = false;
            isAttack = false;
            isIdle = false;
            isStunned = false;
        }
        else
        {
            if (waitTime <= 0)
            {
                waitTime = 0.5f;
                rb.velocity = Vector2.zero;
                isIdle = true;
                currentState = EnemyState.idle;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }

    // If in attacking state, increase walking speed. If attacking, specify delay before transitioning back to idle.
    void Attack()
    {
        if (isAttack)
        {
            waitTime = 1f;
            animator.SetBool("isWalking", true);
            moveSpeed = 200f;
            wallBump = 0;
            horizontal = moveDirection.normalized.x;
            vertical = moveDirection.normalized.y;
            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);
        } else
        {
            if (waitTime < 0)
            {
                isWalk = false;
                isAttack = false;
                moveSpeed = 150f;
                waitTime = 1f;
                wallBump = 0;
                currentState = EnemyState.idle;
            } else
            {
                waitTime -= Time.deltaTime;
            }
        }
        

    }
    // If in walking state, set animations to walking.
    void Walk()
    {
        if (waitTime<=0)
        {
            waitTime = 0.5f;
            currentState = EnemyState.idle;
            animator.SetBool("isWalking", false);
        } else
        {
            waitTime -= Time.deltaTime;
        }
    }

    // If in idle state, cease all movement/animations. After a short delay, pick a random direction and start walking in that direction.
    void Idle()
    {
        if (waitTime <=0)
        {
            
            currentState = EnemyState.walk;
            animator.SetBool("isWalking", true);
            
            isIdle = false;
        } else
        {
            animator.SetBool("isWalking", false);
            waitTime -= Time.deltaTime;
            return;
        }
        if (!isIdle)
        {
            
            isIdle = true;
            if (wallBump==1)
            {
                waitTime = 0.5f;
                vertical = -vertical;
                horizontal = -horizontal;
                playerTrigger.transform.Rotate(new Vector3(0, 0, 180f));
            } else if (wallBump>1)
            {
                waitTime = 0.5f;
                vertical = horizontal;
                horizontal = vertical;
                float rotation;
                if (horizontal!=0)
                {
                    rotation = (horizontal > 0.01f) ? 90f : 270f;
                } else
                {
                    rotation = (vertical > 0.01f) ? 180f : 0f;
                }
                
                
                playerTrigger.transform.localEulerAngles = new Vector3(0f, 0f, rotation);
                wallBump = 0;
            } else
            {
                wallBump = 0;
                switch (Random.Range(0, 4))
                {
                    case 0:
                        // up
                        playerTrigger.transform.localEulerAngles = new Vector3(0f, 0f, 180f);
                        vertical = 1;
                        horizontal = 0;
                        break;
                    case 1:
                        // right
                        playerTrigger.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                        vertical = 0;
                        horizontal = 1;
                        break;
                    case 2:
                        // down
                        playerTrigger.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                        vertical = -1;
                        horizontal = 0;
                        break;
                    case 3:
                        // left
                        playerTrigger.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
                        vertical = 0;
                        horizontal = -1;
                        break;
                    default:
                        break;
                }
            }
            waitTime = Random.Range(1f, 3f);
            moveDirection = new Vector2(horizontal, vertical);
            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);
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

            SoundManager.PlaySound("skeletonDeath");
            StartCoroutine(FlashCo(true));

        }
    }

    // Async coroutine to be called when the enemy is hit.
    // Plays a flashing red animation. If the enemy is supposed to die after being hit, then remove object from the game (by making it inactive).
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
        // If dead, drops a heart 15% of the time and remove self from the game.
        if (death)
        {
            if (Random.Range(0,1000)<15)
            {
                Instantiate(heart, new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y), Quaternion.identity);
            }
            
            this.gameObject.SetActive(false);
        }

    }

}
