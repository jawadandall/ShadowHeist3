using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // State management
    public enum PlayerState
    {
        Normal,
        Crouching,
        Searching
    }
    public PlayerState currentState = PlayerState.Normal;
    
    // Speed variables
    public float normalSpeed = 9f;
    public float crouchSpeed = 4f;
    
    // Sprite references
    public Sprite normalSprite;
    public Sprite crouchingSprite;
    public Sprite searchingSprite;
    private SpriteRenderer spriteRenderer;
    
    // Components
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    
    // Search variables
    public float searchRadius = 1.5f;
    private bool isSearching = false;
    private Vector2 originalColliderSize;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Store original collider size
        originalColliderSize = boxCollider.size;
        
        // Set initial position
        GameObject startPoint = GameObject.Find("StartPoint");
        if (startPoint != null)
        {
            transform.position = startPoint.transform.position;
        }
    }

    void Update()
    {
        // Get movement input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Handle state transitions
        HandleStateTransitions();
        
        // Apply movement based on state
        ApplyMovement(horizontal, vertical);
        
        // Search action
        if (currentState == PlayerState.Searching && Input.GetKeyDown(KeyCode.E))
        {
            Search();
        }
    }
    
    void HandleStateTransitions()
    {
        // Previous state for comparison
        PlayerState previousState = currentState;
        
        // Default to normal state when not pressing special keys
        if (!Input.GetKey(KeyCode.LeftControl) && !isSearching)
        {
            currentState = PlayerState.Normal;
        }
        
        // Crouching state (Left Control)
        if (Input.GetKey(KeyCode.LeftControl) && !isSearching)
        {
            currentState = PlayerState.Crouching;
        }
        
        // Searching state (E key, near interactable)
        if (Input.GetKeyDown(KeyCode.E) && CanSearch() && !isSearching)
        {
            isSearching = true;
            currentState = PlayerState.Searching;
            StartCoroutine(SearchingCoroutine());
        }
        
        // If state changed, update visuals
        if (previousState != currentState)
        {
            UpdatePlayerVisuals();
        }
    }
    
    void ApplyMovement(float horizontal, float vertical)
    {
        // Create movement vector
        Vector2 movementDirection = new Vector2(horizontal, vertical).normalized;
        Vector2 movement = Vector2.zero;
        
        switch (currentState)
        {
            case PlayerState.Normal:
                movement = movementDirection * normalSpeed;
                break;
                
            case PlayerState.Crouching:
                movement = movementDirection * crouchSpeed;
                break;
                
            case PlayerState.Searching:
                // No movement while searching
                movement = Vector2.zero;
                break;
        }
        
        // Apply movement
        rb.linearVelocity = movement;
    }
    
    void UpdatePlayerVisuals()
    {
        // Update sprite based on state
        switch (currentState)
        {
            case PlayerState.Normal:
                spriteRenderer.sprite = normalSprite;
                transform.localScale = new Vector3(1f, 1f, 1f);
                boxCollider.size = originalColliderSize;
                break;
                
            case PlayerState.Crouching:
                spriteRenderer.sprite = crouchingSprite;
                // Reduce height for crouching
                transform.localScale = new Vector3(1f, 0.7f, 1f);
                // Adjust collider size
                boxCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.7f);
                break;
                
            case PlayerState.Searching:
                spriteRenderer.sprite = searchingSprite;
                transform.localScale = new Vector3(1f, 1f, 1f);
                boxCollider.size = originalColliderSize;
                break;
        }
    }
    
    bool CanSearch()
    {
        // Check if there's a lootable object nearby
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        
        foreach (Collider2D obj in nearbyObjects)
        {
            // Check if object has a "Lootable" component or tag
            if (obj.CompareTag("Lootable") || obj.GetComponent<LootableObject>() != null)
            {
                return true;
            }
        }
        
        return false;
    }
    
    void Search()
    {
        // Find nearest lootable object
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, searchRadius);
        
        foreach (Collider2D obj in nearbyObjects)
        {
            LootableObject lootable = obj.GetComponent<LootableObject>();
            if (lootable != null)
            {
                lootable.StartLoot();
                break;
            }
        }
    }
    
    IEnumerator SearchingCoroutine()
    {
        // Wait for search animation
        yield return new WaitForSeconds(1.5f);
        
        // Return to normal state
        isSearching = false;
        currentState = PlayerState.Normal;
        UpdatePlayerVisuals();
    }
    
    // For visualization purposes in the Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}