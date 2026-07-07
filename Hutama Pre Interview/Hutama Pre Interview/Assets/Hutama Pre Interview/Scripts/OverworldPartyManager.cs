using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class OverworldPartyManager : MonoBehaviour
{
    [Header("Character Assignments")]
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private bool giveControlOnStart = true;

    [Header("Follower Delay & Spacing Tuning")]
    [SerializeField] private float followDelayInSeconds = 0.25f;
    [SerializeField] private float followSpacing = 1.2f;
    [SerializeField] private float moveSpeed = 5f;

    private GameObject activeLeader;
    private GameObject activeFollower;
    private Rigidbody2D followerRb;
    private MonoBehaviour p1MovementScript;
    private MonoBehaviour p2MovementScript;

    // Cutscene state machine constants
    private enum CutsceneState { None, WalkingIn, FacingEachOther, WalkingOut }
    private CutsceneState currentCutsceneState = CutsceneState.None;

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
        // === RESTORED INITIALIZATION CODES ===
        if (player1 != null) p1MovementScript = player1.GetComponent<Movement>();
        if (player2 != null) p2MovementScript = player2.GetComponent<Movement>();

        Collider2D c1 = player1 != null ? player1.GetComponent<Collider2D>() : null;
        Collider2D c2 = player2 != null ? player2.GetComponent<Collider2D>() : null;
        if (c1 != null && c2 != null)
        {
            Physics2D.IgnoreCollision(c1, c2, true);
        }
        // =====================================

        if (giveControlOnStart)
        {
            SetActiveCharacter(true);
        }
        else
        {
            if (p1MovementScript != null) p1MovementScript.enabled = false;
            if (p2MovementScript != null) p2MovementScript.enabled = false;
        }
    }

    void Update()
    {
        // Disable manual swapping keys during active cutscenes
        if (currentCutsceneState != CutsceneState.None) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SetActiveCharacter(true);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetActiveCharacter(false);
    }

    void FixedUpdate()
    {
        // Handle Cutscene Movement States Continuously
        if (currentCutsceneState != CutsceneState.None)
        {
            Rigidbody2D p1Rb = player1.GetComponent<Rigidbody2D>();
            Rigidbody2D p2Rb = player2.GetComponent<Rigidbody2D>();

            if (currentCutsceneState == CutsceneState.WalkingIn)
            {
                if (p1Rb != null) p1Rb.linearVelocity = new Vector2(moveSpeed, p1Rb.linearVelocity.y);
                if (p2Rb != null) p2Rb.linearVelocity = new Vector2(moveSpeed, p2Rb.linearVelocity.y);
            }
            else if (currentCutsceneState == CutsceneState.FacingEachOther)
            {
                if (p1Rb != null) p1Rb.linearVelocity = new Vector2(0f, p1Rb.linearVelocity.y);
                if (p2Rb != null) p2Rb.linearVelocity = new Vector2(0f, p2Rb.linearVelocity.y);
            }
            else if (currentCutsceneState == CutsceneState.WalkingOut)
            {
                if (p1Rb != null) p1Rb.linearVelocity = new Vector2(moveSpeed, p1Rb.linearVelocity.y);
                if (p2Rb != null) p2Rb.linearVelocity = new Vector2(moveSpeed, p2Rb.linearVelocity.y);
            }
            return;
        }

        // Normal Gameplay Follower Logic
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

    // --- FUNGUS PUBLIC COORD CONTROL TIE-INS ---

    public void Cutscene_WalkIntoFrame()
    {
        leaderHistory.Clear();
        currentCutsceneState = CutsceneState.WalkingIn;

        player1.transform.localScale = new Vector3(1f, 1f, 1f);
        player2.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void Cutscene_StopAndFaceEachOther()
    {
        currentCutsceneState = CutsceneState.FacingEachOther;

        if (player1 != null) player1.transform.localScale = new Vector3(-1f, 1f, 1f);
        if (player2 != null) player2.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void Cutscene_WalkOffScreenRight()
    {
        currentCutsceneState = CutsceneState.WalkingOut;

        player1.transform.localScale = new Vector3(1f, 1f, 1f);
        player2.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void Cutscene_End()
    {
        currentCutsceneState = CutsceneState.None;
        SetActiveCharacter(true);
    }

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

        if (activeFollower != null) followerRb = activeFollower.GetComponent<Rigidbody2D>();

        if (p1MovementScript != null) p1MovementScript.enabled = choosePlayer1;
        if (p2MovementScript != null) p2MovementScript.enabled = !choosePlayer1;

        // === FIX: Only toggle the leader flag for input, don't assign leaderTransform ===
        if (player1 != null && player1.TryGetComponent(out PartyMemberControl p1Control))
        {
            p1Control.isCurrentLeader = choosePlayer1;
        }

        if (player2 != null && player2.TryGetComponent(out PartyMemberControl p2Control))
        {
            p2Control.isCurrentLeader = !choosePlayer1;
        }
        // ==============================================================================

        if (followerRb != null) followerRb.linearVelocity = new Vector2(0f, followerRb.linearVelocity.y);
    }
}