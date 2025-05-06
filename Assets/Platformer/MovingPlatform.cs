using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Waypoints")]
    [Tooltip("First endpoint of the platform's path")]
    public Transform pointA;
    [Tooltip("Second endpoint of the platform's path")]
    public Transform pointB;

    [Header("Movement")]
    [Tooltip("Speed at which the platform moves (units/sec)")]
    public float speed = 2f;
    [Tooltip("How close to the target before reversing")]
    public float switchThreshold = 0.05f;

    private Vector3 _target;

    void Start()
    {
        // Start moving toward pointB
        if (pointA == null || pointB == null)
        {
            Debug.LogError("MovingPlatform requires two endpoints (pointA and pointB).");
            enabled = false;
            return;
        }
        transform.position = pointA.position;
        _target = pointB.position;
    }

    void FixedUpdate()
    {
        // Move toward current target
        transform.position = Vector3.MoveTowards(
            transform.position,
            _target,
            speed * Time.fixedDeltaTime
        );

        // If close enough, swap target
        if (Vector3.Distance(transform.position, _target) < switchThreshold)
            _target = (_target == pointA.position) ? pointB.position : pointA.position;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Parent the player to this platform so they move together
        if (col.transform.CompareTag("Player"))
            col.transform.SetParent(transform);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        // Un-parent when player leaves
        if (col.transform.CompareTag("Player"))
            col.transform.SetParent(null);
    }

    // Draw gizmos so you can see the path in the editor
    void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.1f);
            Gizmos.DrawSphere(pointB.position, 0.1f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
