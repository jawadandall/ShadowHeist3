// GuardPatrol.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuardPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;         // Array of waypoints
    public float moveSpeed = 2f;          // Movement speed
    public float waypointStopTime = 1f;   // Time to wait at each waypoint
    
    [Header("Vision Settings")]
    public float viewRadius = 5f;         // How far the guard can see
    public float viewAngle = 110f;        // Field of view angle
    public LayerMask targetMask;          // Layer for the player
    public LayerMask obstacleMask;        // Layer for obstacles that block vision
    public float meshResolution = 1f;     // How detailed the FOV mesh is
    public int edgeResolveIterations = 4; // How accurately edge detection works
    public float edgeDistThreshold = 0.5f;// Threshold for edge detection
    
    [Header("References")]
    public Material viewMeshMaterial;     // Material for the view mesh
    
    // Private variables
    private int currentWaypointIndex = 0;
    private bool waiting = false;
    private GameObject viewMeshObj;
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;
    private Transform target;
    
    void Start()
    {
        // Initialize viewMesh for visualization
        viewMeshObj = new GameObject("View Mesh");
        viewMeshObj.transform.parent = transform;
        viewMeshObj.transform.localPosition = new Vector3(0, 0, -1); // Set to -1 local Z
        viewMeshFilter = viewMeshObj.AddComponent<MeshFilter>();
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        
        MeshRenderer viewMeshRenderer = viewMeshObj.AddComponent<MeshRenderer>();
        viewMeshRenderer.material = viewMeshMaterial;
        
        // Set first waypoint
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
    
    void Update()
    {
        // Check for player visibility
        if (CanSeePlayer())
        {
            // Response when player is seen
            Debug.Log("Guard spotted the player!");
            // Add alert behavior here
        }
        
        // Only patrol if not waiting and we have waypoints
        if (!waiting && waypoints.Length > 0)
        {
            Patrol();
        }
        
        // Update vision visualization every frame
        DrawFieldOfView();
    }
    
    void Patrol()
    {
        // Move towards the current waypoint
        transform.position = Vector3.MoveTowards(
            transform.position, 
            waypoints[currentWaypointIndex].position, 
            moveSpeed * Time.deltaTime
        );
        
        // Look in the direction of movement
        Vector3 direction = waypoints[currentWaypointIndex].position - transform.position;
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        // If we've reached the waypoint
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 0.1f)
        {
            // Wait at the waypoint
            StartCoroutine(WaitAtWaypoint());
        }
    }
    
    IEnumerator WaitAtWaypoint()
    {
        waiting = true;
        yield return new WaitForSeconds(waypointStopTime);
        
        // Move to next waypoint
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        waiting = false;
    }
    
    bool CanSeePlayer()
    {
        if (target == null)
            return false;
            
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float distToTarget = Vector3.Distance(transform.position, target.position);
        
        // Check if target is within view radius and angle
        if (distToTarget <= viewRadius)
        {
            float angleToTarget = Vector3.Angle(transform.right, dirToTarget);
            if (angleToTarget <= viewAngle / 2)
            {
                // Check if there's an obstacle between guard and player
                if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            
            // Handle edge detection between raycasts
            if (i > 0)
            {
                bool edgeDistThresholdExceeded = Mathf.Abs(oldViewCast.dist - newViewCast.dist) > edgeDistThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            
            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }
        
        // Create the mesh from the view points
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];
        
        vertices[0] = Vector3.zero; // Center of the mesh is at the guard's position
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);
            
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }
    
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, obstacleMask);
        
        if (hit)
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }
    
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        
        return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
    }
    
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;
        
        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);
            
            bool edgeDistThresholdExceeded = Mathf.Abs(minViewCast.dist - newViewCast.dist) > edgeDistThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDistThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }
        
        return new EdgeInfo(minPoint, maxPoint);
    }
    
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dist;
        public float angle;
        
        public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            dist = _dist;
            angle = _angle;
        }
    }
    
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;
        
        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
    
    // Draw gizmos for debugging in the editor
    void OnDrawGizmos()
    {
        // Draw view radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        
        // Draw view angle
        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
        
        // Draw waypoints and connections
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] != null)
                {
                    // Draw a sphere at each waypoint
                    Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                    
                    // Draw lines between consecutive waypoints
                    if (i < waypoints.Length - 1 && waypoints[i+1] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                    }
                    else if (i == waypoints.Length - 1 && waypoints[0] != null)
                    {
                        // Connect last to first waypoint
                        Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                    }
                }
            }
        }
    }
}