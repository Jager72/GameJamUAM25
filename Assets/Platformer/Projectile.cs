using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float angleDegrees = 0f;


    private float timer;
    private Vector2 direction;
    private BoxCollider2D boxCollider;
    void Start()
    {
        timer = 0f;
        float rad = angleDegrees * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        boxCollider = GetComponent<BoxCollider2D>();
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

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }
}
