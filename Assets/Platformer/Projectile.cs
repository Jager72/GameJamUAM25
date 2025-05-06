using System;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // Expose a static event that listeners (like the spawner) can subscribe to.
    public static event Action OnPlayerHit;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField, Range(0f, 360f)] private float angleDegrees = 0f;

    [Header("Spawn Animation Settings")]
    [SerializeField] private string spawnTrigger = "Spawn";
    [SerializeField] private string flyTrigger = "Fly";
    [SerializeField] private float spawnDuration = 0.5f;

    [Header("Damage Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private string playerTag = "Player";

    [Header("Explosion Settings")]
    [SerializeField] private string explodeTrigger = "Explode";
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

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.useFullKinematicContacts = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        boxCollider.isTrigger = true;
    }

    void Start()
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

        canMove = false;
        if (animator != null && !string.IsNullOrEmpty(spawnTrigger))
            animator.SetTrigger(spawnTrigger);

        StartCoroutine(EnableMovementAfterSpawn());
        timer = 0f;
    }

    private IEnumerator EnableMovementAfterSpawn()
    {
        yield return new WaitForSeconds(spawnDuration);
        canMove = true;
        if (animator != null && !string.IsNullOrEmpty(flyTrigger))
            animator.SetTrigger(flyTrigger);
    }

    void Update()
    {
        if (!canMove || hasHit) return;

        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || !other.CompareTag(playerTag))
            return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            // Fire the static event so the spawner can know a hit occurred
            OnPlayerHit?.Invoke();
        }

        HandleHit();
    }

    private void HandleHit()
    {
        hasHit = true;
        canMove = false;
        boxCollider.enabled = false;

        if (animator != null && !string.IsNullOrEmpty(explodeTrigger))
            animator.SetTrigger(explodeTrigger);

        StartCoroutine(ExplodeAndDestroy());
    }

    private IEnumerator ExplodeAndDestroy()
    {
        yield return new WaitForSeconds(explodeDuration);
        Destroy(gameObject);
    }

    /// <summary>Update firing angle at runtime.</summary>
    public void SetAngle(float angle)
    {
        angleDegrees = angle;
        float rad = angle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);
    }

    /// <summary>Update speed at runtime.</summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
