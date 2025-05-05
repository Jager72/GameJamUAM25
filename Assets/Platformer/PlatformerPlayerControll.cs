using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformerPlayerControll : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    
    private Rigidbody2D rb;
    private float moveInput;
    public bool isGrounded;
    private PlayerInput playerInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        playerInput = GetComponent<PlayerInput>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    // Called by the Input System's PlayerInput component
    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveInput = input.x;
    }
    
    // Called by the Input System's PlayerInput component
    public void OnJump(InputValue value)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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