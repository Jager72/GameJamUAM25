using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField, Range(0f, 360f)] private float angleDegrees = 0f;

    [Header("Spawn Animation Settings")]
    [Tooltip("Animator Trigger to play on spawn.")]
    [SerializeField] private string spawnTrigger = "Spawn";
    [Tooltip("Animator Trigger to switch to flight.")]
    [SerializeField] private string flyTrigger = "Fly";
    [Tooltip("Duration of the spawn animation (seconds).")]
    [SerializeField] private float spawnDuration = 0.5f;

    [Header("Damage Settings")]
    [Tooltip("Damage dealt to the player on hit.")]
    [SerializeField] private int damage = 1;
    [Tooltip("Tag of the player GameObject.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Explosion Settings")]
    [Tooltip("Animator Trigger to play on impact.")]
    [SerializeField] private string explodeTrigger = "Explode";
    [Tooltip("Duration of the explosion animation (seconds).")]
    [SerializeField] private float explodeDuration = 0.3f;

    // cached components
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // runtime state
    private Vector2 direction;
    private float timer;
    private bool canMove = false;
    private bool hasHit = false;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Kinematic so we control it directly
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.useFullKinematicContacts = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Trigger collider
        boxCollider.isTrigger = true;
    }

    void Start()
    {
        // Calculate direction & set facing
        float rad = angleDegrees * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

        // Play spawn animation, then enable movement
        canMove = false;
        if (animator != null && !string.IsNullOrEmpty(spawnTrigger))
            animator.SetTrigger(spawnTrigger);

        StartCoroutine(EnableMovementAfterSpawn());
        timer = 0f;
    }

    private IEnumerator EnableMovementAfterSpawn()
    {
        yield return new WaitForSeconds(spawnDuration);

        // Switch to flying loop
        canMove = true;
        if (animator != null && !string.IsNullOrEmpty(flyTrigger))
            animator.SetTrigger(flyTrigger);
    }

    void Update()
    {
        if (!canMove || hasHit)
            return;

        // Lifetime check
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            DestroyProjectile();
            return;
        }

        // Travel forward along local “up”
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || !other.CompareTag(playerTag))
            return;

        // Deal damage
        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);

        HandleHit();
    }

    private void HandleHit()
    {
        hasHit = true;
        canMove = false;

        // Prevent further collisions
        boxCollider.enabled = false;

        // Play explode animation
        if (animator != null && !string.IsNullOrEmpty(explodeTrigger))
            animator.SetTrigger(explodeTrigger);

        // Wait for the explosion clip then destroy
        StartCoroutine(ExplodeAndDestroy());
    }

    private IEnumerator ExplodeAndDestroy()
    {
        yield return new WaitForSeconds(explodeDuration);
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Update firing angle at runtime.
    /// </summary>
    public void SetAngle(float angle)
    {
        angleDegrees = angle;
        float rad = angle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);
    }

    /// <summary>
    /// Update speed at runtime.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
