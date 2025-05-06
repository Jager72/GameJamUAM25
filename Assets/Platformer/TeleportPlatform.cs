using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TeleportPlatform : MonoBehaviour
{
    [Tooltip("Transform miejsca, do kt�rego gracz zostanie przeniesiony.")]
    [SerializeField] private Transform teleportTarget;

    [Tooltip("Czy zachowa� pr�dko�� gracza po teleportacji?")]
    [SerializeField] private bool preserveVelocity = false;

    private Collider2D _collider;

    void Awake()
    {
        _collider = GetComponent<Collider2D>();
        // Platforma dzia�a jako trigger
        _collider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Teleportujemy pozycj� gracza...
        other.transform.position = teleportTarget.position;

        // ...i opcjonalnie zerujemy lub zachowujemy pr�dko��
        if (!preserveVelocity)
        {
            var rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }

    // W edytorze narysuj lini� do punktu docelowego
    void OnDrawGizmosSelected()
    {
        if (teleportTarget == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, teleportTarget.position);
        Gizmos.DrawSphere(teleportTarget.position, 0.1f);
    }
}
