using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// A generic enum class that is used to identify the player's current state (a state machine)
public enum PlayerState
{
    walk,
    attack,
    interact
}
public class PlayerMovement : MonoBehaviour
{
    public PlayerState currentState;
    public float moveSpeed;
    public bool disableMovement;
    public Animator animator;
    private Vector2 moveDirection;
    
    public Rigidbody2D rb;
    [HideInInspector] private float walkTime;
    public PlayerStats playerStats;
    public PlayerCombat playerCombat;

    [Header("IFrame Stuff")]
    public Color flashColor;
    public Color regularColor;
    public float flashDuration;
    public int numberOfFlashes;
    public SpriteRenderer mySprite;
    private void Awake()
    {
        currentState = PlayerState.walk;
        rb = GetComponent<Rigidbody2D>();
        animator.SetFloat("LastVert", -1);
        playerStats = GetComponent<PlayerStats>();
        mySprite = GetComponent<SpriteRenderer>();
        playerCombat = GetComponent<PlayerCombat>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Physics2D.IgnoreLayerCollision(9, 10, true);
            if (playerStats.health==1)
            {
                SoundManager.PlaySound("playerDeath");
                playerCombat.combatEnabled = false;
                disableMovement = true;
                animator.SetFloat("LastVert", 0);
                animator.SetFloat("LastHorizon", 0);
                animator.SetBool("isAttacking", false);
                StartCoroutine(FlashCo(true));
                StartCoroutine(DeadCo());
                StartCoroutine(CollisionCo(flashDuration * (float)numberOfFlashes * 2));
                FindObjectOfType<GameManager>().EndGame(false);
                // Death animation
            } else
            {
                
                SoundManager.PlaySound("playerHit");
                playerCombat.combatEnabled = false;
                StartCoroutine(FlashCo(false));
                StartCoroutine(CollisionCo(flashDuration * (float) numberOfFlashes * 2));
            }
            
        }
    }
    void ProcessInputs()
    {

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY);

        animator.SetFloat("Horizontal", moveDirection.x);
        animator.SetFloat("Vertical", moveDirection.y);
        animator.SetFloat("Magnitude", moveDirection.magnitude);

        if (moveX > 0.001 || moveX < -0.001)
        {
            if (!disableMovement)
            {
                if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
                {
                    animator.SetFloat("LastHorizon", moveDirection.x);
                    animator.SetFloat("LastVert", 0);
                }
                
            }
            
        }
        if (moveY > 0.001 || moveY < -0.001)
        {
            if (!disableMovement)
            {
                if (Mathf.Abs(moveDirection.x) <= Mathf.Abs(moveDirection.y))
                {
                    animator.SetFloat("LastHorizon", 0);
                    animator.SetFloat("LastVert", moveDirection.y);
                }
        
            }
        }

    }
    
    // Inputs are processed per frame in Update() which is variable, while movement/physics are processed in FixedUpdate where time is constant.
    void Update()
    {
        ProcessInputs();
    }

    // Physics calculations happen at fixed intervals for movement.
    void FixedUpdate()
    {
        Move();
    }

    // Sets player's velocity if the player's movement is not disabled. Plays appropriate sound effects while walking.
    void Move()
    {
        if (!disableMovement)
        {
            // Normalize direction vectors for consistent speed across both the x/y axis.
            moveDirection = moveDirection.normalized;
            // Only move if detect input is non-zero.
            if (walkTime < 0f && moveDirection.sqrMagnitude>0.001f)
            {
                SoundManager.PlaySound("playerWalk");
                Debug.Log("PlayerWalk");
                walkTime = 0.5f;
            }
            walkTime = walkTime - Time.deltaTime;
            rb.velocity = new Vector2((Mathf.Round((moveDirection.x * moveSpeed * Time.deltaTime)/0.0625f)*0.0625f), (Mathf.Round((moveDirection.y * moveSpeed * Time.deltaTime) / 0.0625f)*0.0625f));
        }
        else
        {
            // If movement is disabled, set velocity to 0.
            walkTime = 0.5f;
            rb.velocity = new Vector2(0, 0);
        }
    }

    // Function called by an Attack animation event to disable movement during attacks.
    void DisableMovement()
    {
        disableMovement = true;
    }
    // Function called by an Attack animation event to renenable movement after an attack finishes.
    void EnableMovement()
    {
        disableMovement = false;
    }
    // Async coroutine to be called when the player is hit.
    // Plays a flashing red animation. If the player is supposed to die after being hit, permanently make the character red (in preparation for the Game Over screen).
    private IEnumerator FlashCo(bool isDead)
    {
        int mult = 1;
        // Extend hit animation if dead (flashes for twice as long when dead).
        if (isDead)
        {
            mult = 2;
        }
        // Reduce health by 1.
        playerStats.health -= 1;
        int temp = 0;
        // Alternating between red flashing and original sprite
        while (temp < numberOfFlashes * mult)
        {
            mySprite.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            mySprite.color = regularColor;
            yield return new WaitForSeconds(flashDuration);
            temp++;
        }

        if (isDead) {
            mySprite.color = flashColor;
        }
        
    }

    // Async coroutine to be called when the player is supposed to die.
    // Overrides to idle animation, and manually animate the player to make the sprite spin 360 degrees multiple times.
    private IEnumerator DeadCo()
    {
        // Set animation to idle/dead state.
        animator.SetFloat("Vertical", 0);
        animator.SetFloat("Horizontal", 0);
        animator.SetFloat("Magnitude", 0);
        animator.SetBool("isDead", true);

        // Manually sets animation variables to make the player rotate 90 at fixed time intervals for some number of loops.
        int temp = 0;
        while (temp < numberOfFlashes)
        {
            animator.SetFloat("LastVert", -1);
            animator.SetFloat("LastHorizon", 0);
            yield return new WaitForSeconds(flashDuration / 2);
            animator.SetFloat("LastVert", 0);
            animator.SetFloat("LastHorizon", 1);
            yield return new WaitForSeconds(flashDuration / 2);
            animator.SetFloat("LastVert", 1);
            animator.SetFloat("LastHorizon", 0);
            yield return new WaitForSeconds(flashDuration / 2);
            animator.SetFloat("LastVert", 0);
            animator.SetFloat("LastHorizon", -1);
            yield return new WaitForSeconds(flashDuration / 2);
            temp++;
        }
        // Manually set player's final animation frame to look down (0, -1).
        animator.SetFloat("LastVert", -1);
        animator.SetFloat("LastHorizon", 0);

        // Restores physics collision
        Physics2D.IgnoreLayerCollision(9, 10, false);
    }

    // Async coroutine to temporarily disable all collisions with enemies and disable attacking.
    // Renables collision and the ability to attack after a specified delay.
    private IEnumerator CollisionCo(float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics2D.IgnoreLayerCollision(9, 10, false);
        playerCombat.combatEnabled = true;
    }
}
