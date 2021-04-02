using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonAI : Enemy
{

    // Extra variables (should probably be inherited from the Enemy parent class).
    public Animator animator;
    public Transform target;
    public PolygonCollider2D playerTrigger;
    public GameObject heart;
    public float horizontal;
    public float vertical;
    public Vector2 playerPosition;
    [Header("IFrame Stuff")]
    public Color flashColor;
    public Color regularColor;
    public float flashDuration;
    public int numberOfFlashes;
    public SpriteRenderer mySprite;

    // Initializing all the variables
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
        } else
        {
            isAttack = false;
        }

    }
    // Checks if player is still with the enemy's FOV.
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
        } else
        {
            isAttack = false;
        }
        
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        //if (other.tag == "Player")
        if (other.gameObject.CompareTag("Player"))
        {
            isAttack = false;
        }
    }
    void Attack()
    {
        if (isAttack)
        {
            waitTime = 1f;
            animator.SetBool("isWalking", true);
            moveSpeed = 200f;
            
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
                currentState = EnemyState.idle;
            } else
            {
                waitTime -= Time.deltaTime;
            }
        }
        

    }
    private void FixedUpdate()
    {
        if (currentState.Equals(EnemyState.walk)|| currentState.Equals(EnemyState.attack)||currentState.Equals(EnemyState.stunned))
        {
            moveDirection = moveDirection.normalized;
            rb.velocity = new Vector2((Mathf.Round((moveDirection.x * moveSpeed * Time.deltaTime) / 0.0625f) * 0.0625f), (Mathf.Round((moveDirection.y * moveSpeed * Time.deltaTime) / 0.0625f) * 0.0625f));
        } else
        {
            rb.velocity = new Vector2(0, 0);
        }
    }
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
    void Idle()
    {
        if (waitTime <=0)
        {
            waitTime = Random.Range(1f, 3f);
            currentState = EnemyState.walk;
            animator.SetBool("isWalking", true);
            
            isIdle = false;
        } else
        {
            waitTime -= Time.deltaTime;
            return;
        }
        if (!isIdle)
        {
            isIdle = true;
            
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
            moveDirection = new Vector2(horizontal, vertical);
            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);
        }
    }

    private IEnumerator FlashCo(bool death)
    {
        int temp = 0;
        while (temp < numberOfFlashes)
        {
            mySprite.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            mySprite.color = regularColor;
            yield return new WaitForSeconds(flashDuration);
            temp++;
        }
        if (death)
        {
            if (Random.Range(0,1000)<15)
            {
                Instantiate(heart, new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y), Quaternion.identity);
            }
            
            this.gameObject.SetActive(false);
        }

    }

    public void GetHit()
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
}
