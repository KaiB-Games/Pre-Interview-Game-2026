using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Ground Check Safety")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Gather horizontal walking inputs (A/D or Left/Right arrows)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Check if the player is physically touching the solid floor layer
        if (groundCheckPoint != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
        }
        else
        {
            // Fail-safe backup if the ground check transform slot is empty
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;
        }

        // 3. Jump Trigger Check
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 4. Flip your character sprite dynamically to face the correct way
        if (horizontalInput > 0)
        {
            spriteRenderer.flipX = false; // Facing Right
        }
        else if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;  // Facing Left
        }
    }

    void FixedUpdate()
    {
        // Apply direct horizontal platformer forces to the physics engine
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    // Visual helper lines drawn in editor view to align the floor check circle
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}