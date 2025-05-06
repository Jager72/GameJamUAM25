using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float angleDegrees = 0f;

    private float timer;
    private Vector2 direction;
    private BoxCollider2D boxCollider;
    private Animator animator;

    void Awake()
    {
        // Cache Animator
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("Projectile has no Animator!");
    }

    void Start()
    {
        timer = 0f;
        float rad = angleDegrees * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        boxCollider = GetComponent<BoxCollider2D>();

        // (Optional) explicitly play the flight animation:
        if (animator != null)
            animator.Play("BulletFly");
    }

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

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            if (animator != null)
            {
                // Trigger explosion (or fade-out) animation
                animator.SetTrigger("Explode");
                // Disable further movement
                enabled = false;
                // Destroy after a short delay to let animation play
                Destroy(gameObject, 0.2f);
            }
            else
            {
                Destroy(gameObject);
            }
            return;
        }

        // Move along local up as before
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }
}
