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
    [Tooltip("Name of the Animator trigger to play on spawn.")]
    [SerializeField] private string spawnTrigger = "Spawn";
    [Tooltip("Name of the Animator trigger to switch to flight.")]
    [SerializeField] private string flyTrigger = "Fly";
    [Tooltip("How long (seconds) the spawn animation takes.")]
    [SerializeField] private float spawnDuration = 0.5f;

    [Header("Damage Settings")]
    [SerializeField] private int damage = 1;
    [SerializeField] private string playerTag = "Player";

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private Animator animator;

    private Vector2 direction;
    private float timer;
    private bool hasHit;
    private bool canMove;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.useFullKinematicContacts = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        boxCollider.isTrigger = true;
    }

    void Start()
    {
        // compute facing direction
        float rad = angleDegrees * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

        // 1) Don't move yet
        canMove = false;

        // 2) Play spawn animation
        if (animator != null && !string.IsNullOrEmpty(spawnTrigger))
            animator.SetTrigger(spawnTrigger);

        // 3) After its duration, allow movement and switch to flight anim
        StartCoroutine(EnableMovementAfterSpawn());

        // init lifetime timer
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
        if (hasHit) return;

        // only start moving once spawn anim has played out
        if (!canMove) return;

        // handle lifetime
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            DestroyProjectile();
            return;
        }

        // move forward in local “up”
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        if (!other.CompareTag(playerTag)) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);

        HandleHit();
    }

    private void HandleHit()
    {
        hasHit = true;
        enabled = false;
        boxCollider.enabled = false;
        // you could play a hit/explosion trigger here
        Invoke(nameof(DestroyProjectile), 0.2f);
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// If you need to change angle at runtime, update direction and transform
    /// </summary>
    public void SetAngle(float angle)
    {
        angleDegrees = angle;
        float rad = angle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
