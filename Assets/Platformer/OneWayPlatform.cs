using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class OneWayPlatform : MonoBehaviour
{
    private Collider2D _platformCollider;
    private HashSet<Collider2D> _ignoredPlayers = new HashSet<Collider2D>();

    void Awake()
    {
        _platformCollider = GetComponent<Collider2D>();
        // Ensure normal collisions by default
        _platformCollider.isTrigger = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        var other = collision.collider;
        if (_ignoredPlayers.Contains(other))
        {
            Physics2D.IgnoreCollision(_platformCollider, other, false);
            _ignoredPlayers.Remove(other);
        }
    }

    private void HandleCollision(Collision2D collision)
    {
        var other = collision.collider;
        if (!other.CompareTag("Player"))
            return;

        // Determine platform top Y
        float platformTop = _platformCollider.bounds.max.y;

        // Determine player's bottom Y
        float playerBottom = other.bounds.min.y;

        // If player is below the platform top (approaching from underneath), ignore collision
        if (playerBottom >= platformTop)
            return; // player is at or above, keep collision

        // Also ensure the player is moving upward (jumping through)
        Rigidbody2D rb = collision.rigidbody;
        if (rb != null && rb.linearVelocity.y > 0f)
        {
            if (!_ignoredPlayers.Contains(other))
            {
                Physics2D.IgnoreCollision(_platformCollider, other, true);
                _ignoredPlayers.Add(other);
            }
        }
        else if (_ignoredPlayers.Contains(other))
        {
            // Player has fallen back down or is no longer ascending: restore collision
            Physics2D.IgnoreCollision(_platformCollider, other, false);
            _ignoredPlayers.Remove(other);
        }
    }
}
