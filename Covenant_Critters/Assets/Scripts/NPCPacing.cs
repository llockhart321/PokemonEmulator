using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPacing : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float pauseDuration = 3f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private bool useCustomSpriteAnimation = true;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Sprite Animation Sequences")]
    [SerializeField] private List<Sprite> upSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> downSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> leftSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> rightSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> idleSprites = new List<Sprite>();
    [SerializeField] private float frameRate = 8f; // Frames per second for sprite animation
    
    // Movement direction enum
    private enum Direction { Up, Down, Left, Right, Idle }
    private Direction currentDirection = Direction.Idle;
    
    // Movement control
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool isPaused = false;
    
    [Header("Movement Range")]
    [SerializeField] private float movementDistance = 2f;
    [SerializeField] private float maxDistanceFromStart = 5f;
    
    // Collision detection
    [SerializeField] private LayerMask collisionLayers;
    private Vector2 startPosition;
    private Rigidbody2D rb;
    private Collider2D npcCollider;
    
    // Sprite animation variables
    private int currentSpriteIndex = 0;
    private float timeSinceLastFrameChange = 0f;
    private float frameDelay;
    private List<Sprite> currentSpriteSequence;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // For top-down 2D games, usually you'll want this
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        npcCollider = GetComponent<Collider2D>();
        if (npcCollider == null)
        {
            // Add a collider if none exists - adjust size as needed for your sprites
            var boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = false;
            boxCollider.size = new Vector2(0.8f, 0.8f);
        }
        
        // Get SpriteRenderer if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Calculate frame delay from frame rate
        frameDelay = 1f / frameRate;
        
        // Set initial sprite sequence
        currentSpriteSequence = idleSprites.Count > 0 ? idleSprites : null;
    }
    
    private void Start()
    {
        // Store initial position
        startPosition = transform.position;
        targetPosition = startPosition;
        
        // Set default collision layers if not specified
        if (collisionLayers.value == 0)
        {
            collisionLayers = LayerMask.GetMask("Default");
        }
        
        // Start the movement routine
        StartCoroutine(PacingRoutine());
    }
    
    private void Update()
    {
        if (isMoving && !isPaused)
        {
            // Calculate movement in this frame
            Vector2 movement = Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime) - rb.position;
            
            // Move the rigidbody (using MovePosition is better for physics)
            rb.MovePosition(rb.position + movement);
            
            // Check if we've reached the target position
            if (Vector2.Distance(rb.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                StartCoroutine(PauseBeforeNextMove());
            }
        }
        
        // Update sprite animation if using custom sprites
        if (useCustomSpriteAnimation && spriteRenderer != null && currentSpriteSequence != null && currentSpriteSequence.Count > 0)
        {
            UpdateSpriteAnimation();
        }
    }
    
    private void UpdateSpriteAnimation()
    {
        // Increment timer
        timeSinceLastFrameChange += Time.deltaTime;
        
        // Check if it's time to change frames
        if (timeSinceLastFrameChange >= frameDelay)
        {
            // Reset timer
            timeSinceLastFrameChange = 0f;
            
            // Move to next frame
            currentSpriteIndex = (currentSpriteIndex + 1) % currentSpriteSequence.Count;
            
            // Update sprite
            spriteRenderer.sprite = currentSpriteSequence[currentSpriteIndex];
        }
    }
    
    private IEnumerator PacingRoutine()
    {
        while (true)
        {
            if (!isMoving && !isPaused)
            {
                // Choose random direction
                ChooseRandomDirection();
                isMoving = true;
            }
            yield return null;
        }
    }
    
    private IEnumerator PauseBeforeNextMove()
    {
        // Set to idle state
        SetDirection(Direction.Idle);
        isPaused = true;
        
        // Wait for pause duration
        yield return new WaitForSeconds(pauseDuration);
        
        // Resume movement
        isPaused = false;
    }
    
    private void ChooseRandomDirection()
    {
        // Try up to 5 times to find a valid direction
        for (int attempt = 0; attempt < 5; attempt++)
        {
            // Pick a random direction (excluding Idle)
            Direction newDirection = (Direction)Random.Range(0, 4);
            
            // Calculate potential target position based on direction
            Vector2 potentialTarget = CalculateTargetPosition(newDirection);
            
            // Check if the target is within range from start position
            if (Vector2.Distance(potentialTarget, startPosition) > maxDistanceFromStart)
            {
                continue; // Try another direction
            }
            
            // Check for collisions
            if (IsPathClear(rb.position, potentialTarget))
            {
                // Set the direction
                SetDirection(newDirection);
                targetPosition = potentialTarget;
                return;
            }
        }
        
        // If all attempts failed, just idle or try to move back toward starting position
        if (Vector2.Distance(rb.position, startPosition) > 0.1f)
        {
            // Move back toward start position
            Direction directionToStart = GetDirectionTowardsPoint(startPosition);
            SetDirection(directionToStart);
            targetPosition = CalculateTargetPosition(directionToStart);
        }
        else
        {
            // Just idle
            SetDirection(Direction.Idle);
            StartCoroutine(PauseBeforeNextMove());
        }
    }
    
    private Direction GetDirectionTowardsPoint(Vector2 point)
    {
        Vector2 direction = point - rb.position;
        
        // Determine which direction has the largest component
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return direction.y > 0 ? Direction.Up : Direction.Down;
        }
    }
    
    private Vector2 CalculateTargetPosition(Direction direction)
    {
        Vector2 currentPos = rb.position;
        switch (direction)
        {
            case Direction.Up:
                return currentPos + new Vector2(0, movementDistance);
            case Direction.Down:
                return currentPos + new Vector2(0, -movementDistance);
            case Direction.Left:
                return currentPos + new Vector2(-movementDistance, 0);
            case Direction.Right:
                return currentPos + new Vector2(movementDistance, 0);
            default:
                return currentPos;
        }
    }
    
    private bool IsPathClear(Vector2 from, Vector2 to)
    {
        // Get distance and direction
        float distance = Vector2.Distance(from, to);
        Vector2 direction = (to - from).normalized;
        
        // Check if there are any collisions in the path
        RaycastHit2D hit = Physics2D.Raycast(from, direction, distance, collisionLayers);
        
        // Draw debug rays to help visualize in the scene view
        Debug.DrawRay(from, direction * distance, hit.collider ? Color.red : Color.green, 0.5f);
        
        // No collision if hit.collider is null
        return hit.collider == null;
    }
    
    private void SetDirection(Direction direction)
    {
        currentDirection = direction;
        
        // Reset animation frame index when changing direction
        currentSpriteIndex = 0;
        timeSinceLastFrameChange = 0f;
        
        // Update animation or sprite based on direction
        if (animator != null && !useCustomSpriteAnimation)
        {
            // If using Animator
            animator.SetBool("Walking", direction != Direction.Idle);
            
            // Set direction parameters in animator
            animator.SetBool("WalkingUp", direction == Direction.Up);
            animator.SetBool("WalkingDown", direction == Direction.Down);
            animator.SetBool("WalkingLeft", direction == Direction.Left);
            animator.SetBool("WalkingRight", direction == Direction.Right);
        }
        else if (spriteRenderer != null && useCustomSpriteAnimation)
        {
            // If using custom sprite animation
            switch (direction)
            {
                case Direction.Up:
                    currentSpriteSequence = upSprites.Count > 0 ? upSprites : null;
                    break;
                case Direction.Down:
                    currentSpriteSequence = downSprites.Count > 0 ? downSprites : null;
                    break;
                case Direction.Left:
                    currentSpriteSequence = leftSprites.Count > 0 ? leftSprites : null;
                    break;
                case Direction.Right:
                    currentSpriteSequence = rightSprites.Count > 0 ? rightSprites : null;
                    break;
                case Direction.Idle:
                    currentSpriteSequence = idleSprites.Count > 0 ? idleSprites : null;
                    break;
            }
            
            // Set initial sprite if sequence exists
            if (currentSpriteSequence != null && currentSpriteSequence.Count > 0)
            {
                spriteRenderer.sprite = currentSpriteSequence[0];
            }
        }
    }
    
    // For debugging - draw the max distance range in the scene view
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPosition, maxDistanceFromStart);
        }
    }
}