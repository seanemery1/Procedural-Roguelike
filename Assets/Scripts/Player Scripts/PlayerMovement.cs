using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
    
    private void Awake()
    {
        currentState = PlayerState.walk;
        rb = GetComponent<Rigidbody2D>();
        animator.SetFloat("LastVert", -1);
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
    // Update is called once per frame
    void Update()
    {
        // Inputs
        ProcessInputs();
        
        
        //transform.position = transform.position + movement * Time.deltaTime;

    }

    void Move()
    {
        if (!disableMovement)
        {
            //if (moveDirection.sqrMagnitude > 1) // Only normalize if necessary
            //{
               moveDirection = moveDirection.normalized;
            //}
            rb.velocity = new Vector2((Mathf.Round((moveDirection.x * moveSpeed * Time.deltaTime)/0.0625f)*0.0625f), (Mathf.Round((moveDirection.y * moveSpeed * Time.deltaTime) / 0.0625f)*0.0625f));
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }

        
        //Vector2 temp = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
        //rb.MovePosition(PixelPerfectClamp(rb.position, 16) + PixelPerfectClamp(temp, 16));
    }

    void FixedUpdate()
    {
        // Physics
        
            Move();
        
        
    }
    private Vector2 PixelPerfectClamp(Vector2 moveVector, float pixelsPerUnit)
    {
        Vector2 vectorInPixels = new Vector2(Mathf.RoundToInt(moveVector.x * pixelsPerUnit), Mathf.RoundToInt(moveVector.x * pixelsPerUnit));

        return vectorInPixels / pixelsPerUnit;
    }
    void DisableMovement()
    {
        disableMovement = true;
    }
    void EnableMovement()
    {
        disableMovement = false;
    }
}
