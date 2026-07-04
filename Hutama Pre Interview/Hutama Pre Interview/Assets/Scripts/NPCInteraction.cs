using UnityEngine;
using Fungus;


public class NPCInteraction : MonoBehaviour {

    [SerializeField] private Flowchart flowchart;
    [SerializeField] private string blockName = "NPC Conversation";
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private SpriteRenderer npcSprite;

    private bool playerInRange = false;
    private bool isTalking = false;

    void Start()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TriggerConversation();
        }
    }

    void TriggerConversation()
    {

        isTalking = true;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        FacePlayer();

        if (flowchart != null)
        {
            flowchart.ExecuteBlock(blockName);
        }
    }

    void FacePlayer()
    {
        if (playerTransform == null || npcSprite == null) return;

        if (playerTransform.position.x < transform.position.x)
        {
            npcSprite.flipX = true; 
        }
        else
        {
            npcSprite.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (playerTransform == null) playerTransform = collision.transform;

            if (!isTalking && interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            isTalking = false;

            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }
}