using System;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Maximum health of the player.")]
    [SerializeField] private int maxHealth = 5;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // Expose a static event that listeners (like the spawner) can subscribe to.
    public static event Action OnPlayerDead;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerHealth requires a SpriteRenderer!");
            enabled = false;
            return;
        }
        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;
        UpdateColor();
    }

    /// <summary>
    /// Apply damage to the player. Clamps at zero and updates color.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
            return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateColor();

        if (currentHealth == 0)
            Die();
    }

    /// <summary>
    /// Heal the player by the specified amount. Clamps at maxHealth.
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0 || currentHealth >= maxHealth)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateColor();
    }

    /// <summary>
    /// Updates the sprite color based on current health percentage.
    /// Full health = original color, zero health = pure red.
    /// </summary>
    private void UpdateColor()
    {
        float t = maxHealth >= 0 ? (float)currentHealth / maxHealth : 0f;
        // Lerp from red (0 health) to originalColor (full health)
        spriteRenderer.color = Color.Lerp(Color.red, originalColor, t);
    }

    /// <summary>
    /// Called when health reaches zero.
    /// </summary>
    private void Die()
    {
        ProjectileSpawner PS = FindAnyObjectByType<ProjectileSpawner>();
        OnPlayerDead?.Invoke();
        Debug.Log("Player has died.");
    }
}
