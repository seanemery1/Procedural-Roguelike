using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public Animator animator;
    private Vector2 moveDirection;
    public Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY);

        animator.SetFloat("Horizontal", moveDirection.x);
        animator.SetFloat("Vertical", moveDirection.y);
        animator.SetFloat("Magnitude", moveDirection.magnitude);

        if (animator.GetFloat("LastHorizon") > 0.001 & animator.GetFloat("LastHorizon") < -0.001)
        {
            animator.SetFloat("LastHorizon", moveDirection.x);
        }
        if (animator.GetFloat("LastVert") > 0.001 & animator.GetFloat("LastVert") < -0.001)
        {
            animator.SetFloat("LastVert", moveDirection.y);
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
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
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
}
