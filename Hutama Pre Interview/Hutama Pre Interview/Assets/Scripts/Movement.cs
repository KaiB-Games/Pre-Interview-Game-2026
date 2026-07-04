using UnityEngine;

public class Movement : MonoBehaviour
{
    //Variable Initialization
    public Rigidbody2D playerRb;
    public float speed;
    public float input;
    public SpriteRenderer spriteRenderer;
    public float jumpForce;
    private bool isGrounded;

    private void Update()
    {
        if (TryGetComponent(out PartyMemberControl control))
        {
            // If character is not the leader, then it blocks all inputs
            if (!control.isCurrentLeader) return;
        }

        //Movement and Placement scripts
        input = Input.GetAxisRaw("Horizontal");
        if (input < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (input > 0)
        {
            spriteRenderer.flipX = false;
        }
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate() //Runs every second (50 times)
    {
        playerRb.linearVelocity = new Vector2(input * speed, playerRb.linearVelocity.y);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
