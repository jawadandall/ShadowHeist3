using UnityEngine;

public class ImprovedCameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    
    // Camera bounds to keep within level
    public bool useBounds = true;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;
    
    // Look ahead in movement direction
    public float lookAheadDistance = 2f;
    
    // Camera zoom (orthographic size)
    public float cameraSize = 5f;
    
    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    
    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
        }
        
        cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = cameraSize;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Get player movement direction
        Vector2 playerDirection = Vector2.zero;
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            playerDirection = rb.linearVelocity.normalized;
        }
        
        // Look ahead in movement direction
        Vector3 lookAhead = new Vector3(playerDirection.x, playerDirection.y, 0) * lookAheadDistance;
        
        // Calculate desired position
        Vector3 desiredPosition = target.position + offset + lookAhead;
        
        // Apply bounds
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }
        
        // Smooth camera movement
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
    }
}