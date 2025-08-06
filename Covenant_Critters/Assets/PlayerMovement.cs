using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    float horizontal;
    float vertical;
    public float moveSpeed;
    public float runSpeed = 20.0f;
    private Vector2 moveDirection;
    
    // Add animator reference
    private Animator animator;
    
    // Store last direction for idle animations
    private float lastMoveX = 0;
    private float lastMoveY = -1; // Default to facing down
    private bool facingHorizontal = false; // Track if last movement was horizontal
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Initialize animation params with default direction
        animator.SetFloat("moveX", lastMoveX);
        animator.SetFloat("moveY", lastMoveY);
        animator.SetBool("isMoving", false);
    }
    
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        
        // Check if there's any movement input
        bool isMoving = (horizontal != 0 || vertical != 0);
        
        // Set the isMoving parameter for animations
        animator.SetBool("isMoving", isMoving);
        
        if (isMoving)
        {
            // Determine facing direction priority
            if (horizontal != 0)
            {
                lastMoveX = horizontal;
                lastMoveY = 0;
                facingHorizontal = true;
            }
            else if (vertical != 0)
            {
                lastMoveX = 0;
                lastMoveY = vertical;
                facingHorizontal = false;
            }
        }
        
        // Always set animator values to last direction
        animator.SetFloat("moveX", lastMoveX);
        animator.SetFloat("moveY", lastMoveY);
    }
    
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
    }
    
    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;
    }
    
    void Move()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }
}
