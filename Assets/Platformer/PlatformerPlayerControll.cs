using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlatformerPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 20f;
    public float deceleration = 30f;
    [Range(0f, 1f)] public float airControlMultiplier = 0.5f;

    [Header("Jumping")]
    public float jumpForce = 10f;
    [Tooltip("How long after leaving a platform you can still jump.")]
    public float coyoteTime = 0.2f;
    [Tooltip("How long before landing you can press jump early.")]
    public float jumpBufferTime = 0.2f;
    [Tooltip("Multiplier to cut jump short when jump button is released.")]
    public float jumpCutMultiplier = 0.5f;

    [Header("Dash")]
    [Tooltip("Impulse force applied when dashing.")]
    public float dashForce = 20f;
    [Tooltip("Cooldown before you can dash again.")]
    public float dashCooldown = 1f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private float moveInput;
    private float facingDirection = 1f;  // +1 = right, -1 = left

    // Timers & state
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool jumpBuffered;
    private bool isGrounded;

    private float dashCooldownTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        // Ground Check
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Coyote Time
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump Buffering
        if (jumpBuffered)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpBuffered = false;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Execute Buffered Jump
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // Dash Cooldown
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Smooth horizontal movement
        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;

        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f)
            ? acceleration
            : deceleration;

        accelRate *= isGrounded ? 1f : airControlMultiplier;

        Vector2 force = Vector2.right * speedDiff * accelRate;
        rb.AddForce(force);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>().x;
        if (moveInput > 0f) facingDirection = 1f;
        else if (moveInput < 0f) facingDirection = -1f;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBuffered = true;
        }
        else if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }

    public void OnSprint(InputValue value)
    {
        if (value.isPressed && dashCooldownTimer <= 0f)
        {
            // Instantaneous dash impulse
            rb.AddForce(Vector2.right * facingDirection * dashForce, ForceMode2D.Impulse);
            dashCooldownTimer = dashCooldown;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
