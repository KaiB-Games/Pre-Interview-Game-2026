using UnityEngine;

public class PartyMemberControl : MonoBehaviour
{
    [HideInInspector] public bool isCurrentLeader = false;
    [HideInInspector] public Transform leaderTransform;

    [Header("Follower Settings")]
    public float followSpacing = 2.0f;
    public float moveSpeed = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        //if the character is the current leader, do nothing.
        if (isCurrentLeader) return;

        // tailing movement handler
        if (leaderTransform == null) return;

        //Variable Initialization
        float leaderDirection = Mathf.Sign(leaderTransform.localScale.x);
        float targetX = leaderTransform.transform.position.x - (leaderDirection * followSpacing);
        float currentX = transform.position.x;
        float distanceX = targetX - currentX;

        if (Mathf.Abs(distanceX) > 0.15f)
        {
            float directionX = Mathf.Sign(distanceX);
            rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(directionX, 1, 1);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            transform.localScale = leaderTransform.localScale;
        }
    }
}