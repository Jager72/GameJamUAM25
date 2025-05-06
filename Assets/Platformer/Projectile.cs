using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField, Range(0f, 360f)] private float angleDegrees = 0f;

    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt to the player on hit.")]
    [SerializeField] private int damage = 1;
    [Tooltip("Player tag to detect collisions.")]
    [SerializeField] private string playerTag = "Player";

    private float timer;
    private Vector2 direction;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private Animator animator;
    private bool hasHit = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Ensure Rigidbody2D is kinematic so movement via Transform works
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;
        rb.useFullKinematicContacts = true;

        // Make sure collider is trigger
        if (boxCollider != null)
            boxCollider.isTrigger = true;
    }

    void Start()
    {
        timer = 0f;
        float rad = angleDegrees * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);

        // Play flight animation if present
        //if (animator != null)
            //animator.Play("BulletFly");
    }

    void Update()
    {
        if (hasHit) return;

        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            DestroyProjectile();
            return;
        }

        // Move projectile along its facing direction
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1) Only proceed if this really is the Player
        if (!other.CompareTag(playerTag) || hasHit)
            return;

        // 2) (Optional) log a hit for debugging
        Debug.Log("Projectile hit Player!");

        // 3) Actually deal damage
        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);

        // 4) Explode & destroy
        HandleHit();
    }

    /// <summary>
    /// Handles the hit: triggers explosion, disables movement/collider, then destroys.
    /// </summary>
    private void HandleHit()
    {
        hasHit = true;
        // Disable movement
        enabled = false;
        // Disable collider
        if (boxCollider != null)
            boxCollider.enabled = false;
        // Play explosion if animator exists
        //if (animator != null)
            //animator.SetTrigger("Explode");
        // Destroy after a delay to allow animation
        Invoke(nameof(DestroyProjectile), 0.2f);
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Sets the firing angle (in degrees) at runtime.
    /// </summary>
    public void SetAngle(float angle)
    {
        angleDegrees = angle;
        float rad = angle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.rotation = Quaternion.Euler(0f, 0f, angleDegrees);
    }

    /// <summary>
    /// Sets the projectile speed at runtime.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                boxCollider.bounds.center,
                boxCollider.bounds.size
            );
        }
    }
}
