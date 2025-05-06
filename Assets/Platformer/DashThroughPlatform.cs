using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class DashThroughPlatform : MonoBehaviour
{
    [Tooltip("Color to flash when the player can dash through.")]
    public Color highlightColor = Color.cyan;

    private Collider2D _col;
    private SpriteRenderer _sr;
    private Color _originalColor;
    private Coroutine _dashRoutine;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        _sr = GetComponent<SpriteRenderer>();
        _originalColor = _sr.color;
    }

    void OnEnable()
    {
        PlatformerPlayerController.OnDashEvent += HandleOnDash;
    }

    void OnDisable()
    {
        PlatformerPlayerController.OnDashEvent -= HandleOnDash;
    }

    private void HandleOnDash(float window)
    {
        // restart the routine if you dash again quickly
        if (_dashRoutine != null)
            StopCoroutine(_dashRoutine);

        _dashRoutine = StartCoroutine(DashThroughRoutine(window));
    }

    private IEnumerator DashThroughRoutine(float window)
    {
        // 1) flash and switch to trigger
        _sr.color = highlightColor;
        _col.isTrigger = true;

        // 2) wait for the dash‐through window
        yield return new WaitForSeconds(window);

        // 3) revert
        _col.isTrigger = false;
        _sr.color = _originalColor;
        _dashRoutine = null;
    }

    // optional: visualize in the editor
    void OnDrawGizmosSelected()
    {
        if (_col != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_col.bounds.center, _col.bounds.size);
        }
    }
}
