using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class CrumblingPlatform : MonoBehaviour
{
    [Header("Timings")]
    [Tooltip("Time in seconds to wait after player lands before starting color fade.")]
    public float delayBeforeAlert = 0.2f;
    [Tooltip("Duration in seconds over which to fade to alertColor.")]
    public float colorChangeDuration = 0.5f;
    [Tooltip("Time in seconds to stay fully alertColor before vanishing.")]
    public float crumbleDelay = 0.5f;
    [Tooltip("Time in seconds the platform remains vanished before respawning.")]
    public float respawnDelay = 5f;

    [Header("Visuals")]
    [Tooltip("Color to fade to when about to crumble.")]
    public Color alertColor = Color.red;

    private SpriteRenderer _sprite;
    private Collider2D _collider;
    private Color _originalColor;
    private bool _triggered;

    void Awake()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _originalColor = _sprite.color;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (_triggered) return;
        if (!other.collider.CompareTag("Player")) return;

        _triggered = true;
        StartCoroutine(CrumbleRoutine());
    }

    private IEnumerator CrumbleRoutine()
    {
        // 1) Wait before starting fade
        yield return new WaitForSeconds(delayBeforeAlert);

        // 2) Gradually lerp the color
        float timer = 0f;
        while (timer < colorChangeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / colorChangeDuration);
            _sprite.color = Color.Lerp(_originalColor, alertColor, t);
            yield return null;
        }
        _sprite.color = alertColor;

        // 3) Hold fully red
        yield return new WaitForSeconds(crumbleDelay);

        // 4) Vanish (disable collider & sprite)
        _collider.enabled = false;
        _sprite.enabled = false;

        // 5) Wait respawnDelay, then re-enable
        yield return new WaitForSeconds(respawnDelay);

        _sprite.enabled = true;
        _collider.enabled = true;
        _sprite.color = _originalColor;
        _triggered = false;
    }

    void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
