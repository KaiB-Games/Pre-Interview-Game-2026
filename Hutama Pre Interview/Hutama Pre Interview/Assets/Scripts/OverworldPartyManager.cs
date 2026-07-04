using UnityEngine;
using System.Collections.Generic;

public class OverworldPartyManager : MonoBehaviour
{
    [Header("Character Assignments")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    [Header("Follower Delay & Spacing Tuning")]
    [SerializeField] private float followDelayInSeconds = 0.25f; // Delay where follower mimics player
    [SerializeField] private float followSpacing = 1.2f;         // Stops overlapping the follower with party leader
    [SerializeField] private float moveSpeed = 5f;              // Match speed of follower with player

    //Variable Initialization
    private GameObject activeLeader;
    private GameObject activeFollower;
    private Rigidbody2D followerRb;
    private MonoBehaviour p1MovementScript;
    private MonoBehaviour p2MovementScript;

    private struct MovementSnapshot
    {
        public Vector3 position;
        public Vector3 localScale;
        public float timeRecorded;

        public MovementSnapshot(Vector3 pos, Vector3 scale, float time)
        {
            position = pos;
            localScale = scale;
            timeRecorded = time;
        }
    }

    private Queue<MovementSnapshot> leaderHistory = new Queue<MovementSnapshot>();
    private Vector3 targetDelayedPosition;
    private Vector3 targetDelayedScale;

    void Start()
    {
        //Make sure input is enabled for active member
        if (player1 != null) p1MovementScript = player1.GetComponent("Movement") as MonoBehaviour;
        if (player2 != null) p2MovementScript = player2.GetComponent("Movement") as MonoBehaviour;

        Collider2D c1 = player1 != null ? player1.GetComponent<Collider2D>() : null;
        Collider2D c2 = player2 != null ? player2.GetComponent<Collider2D>() : null;
        if (c1 != null && c2 != null)
        {
            Physics2D.IgnoreCollision(c1, c2, true);
        }

        SetActiveCharacter(true);
    }

    void Update() //Updates for party member switcher
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveCharacter(true);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveCharacter(false);
    }

    void FixedUpdate()
    {
        //Fix follower positioning and speed
        if (activeLeader == null || activeFollower == null || followerRb == null) return;

        leaderHistory.Enqueue(new MovementSnapshot(
            activeLeader.transform.position,
            activeLeader.transform.localScale,
            Time.time
        ));

        float targetTime = Time.time - followDelayInSeconds;
        while (leaderHistory.Count > 0 && leaderHistory.Peek().timeRecorded <= targetTime)
        {
            MovementSnapshot oldestSnapshot = leaderHistory.Dequeue();
            targetDelayedPosition = oldestSnapshot.position;
            targetDelayedScale = oldestSnapshot.localScale;
        }

        if (targetDelayedPosition != Vector3.zero)
        {
            float leaderFacingDirection = Mathf.Sign(targetDelayedScale.x);
            float adjustedTargetX = targetDelayedPosition.x - (leaderFacingDirection * followSpacing);

            float distanceX = adjustedTargetX - activeFollower.transform.position.x;

            if (Mathf.Abs(distanceX) > 0.15f)
            {
                float directionX = Mathf.Sign(distanceX);
                followerRb.linearVelocity = new Vector2(directionX * moveSpeed, followerRb.linearVelocity.y);
                activeFollower.transform.localScale = new Vector3(directionX, 1, 1);
            }
            else
            {
                followerRb.linearVelocity = new Vector2(0f, followerRb.linearVelocity.y);
                activeFollower.transform.localScale = targetDelayedScale;
            }
        }
    }

    //Character Switcher
    void SetActiveCharacter(bool choosePlayer1)
    {
        leaderHistory.Clear();
        targetDelayedPosition = Vector3.zero;

        if (choosePlayer1)
        {
            activeLeader = player1;
            activeFollower = player2;
        }
        else
        {
            activeLeader = player2;
            activeFollower = player1;
        }

        if (activeFollower != null)
        {
            followerRb = activeFollower.GetComponent<Rigidbody2D>();
        }

        if (p1MovementScript != null) p1MovementScript.enabled = choosePlayer1;
        if (p2MovementScript != null) p2MovementScript.enabled = !choosePlayer1;

        if (followerRb != null) followerRb.linearVelocity = new Vector2(0f, followerRb.linearVelocity.y);
        if (activeLeader != null && activeLeader.TryGetComponent(out Rigidbody2D leaderRb))
        {
            leaderRb.linearVelocity = new Vector2(0f, leaderRb.linearVelocity.y);
        }

        var vcam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null && activeLeader != null)
        {
            vcam.Follow = activeLeader.transform;
        }
    }
}