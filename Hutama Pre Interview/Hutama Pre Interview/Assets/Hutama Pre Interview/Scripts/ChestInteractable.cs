using UnityEngine;

public class ChestInteractable : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject promptTextObject; // Drag "IgnoreText" (old Chest Text) here
    public GameObject rewardTextObject; // Drag your brand new "Reward Text" here

    private bool isPlayerNearby = false;
    private bool isOpened = false;
    private Cainos.PixelArtPlatformer_VillageProps.Chest builtInChestScript;

    void Start()
    {
        builtInChestScript = GetComponent<Cainos.PixelArtPlatformer_VillageProps.Chest>();

        // Hide both at start
        if (promptTextObject != null) promptTextObject.SetActive(false);
        if (rewardTextObject != null) rewardTextObject.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && !isOpened && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;

        // 1. Open the chest animation via the package script
        if (builtInChestScript != null)
        {
            builtInChestScript.Open();
        }

        // 2. Hide the [E] prompt permanently
        if (promptTextObject != null) promptTextObject.SetActive(false);

        // 3. Show your custom reward text pop-up
        if (rewardTextObject != null)
        {
            rewardTextObject.SetActive(true);
            // Hide it automatically after 3 seconds
            Invoke("HideReward", 3.0f);
        }

        Debug.Log("[CHEST] Player found a potion! Healed for 50 HP.");
    }

    void HideReward()
    {
        if (rewardTextObject != null) rewardTextObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpened && (other.CompareTag("Player") || other.name == "Player"))
        {
            isPlayerNearby = true;
            if (promptTextObject != null) promptTextObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.name == "Player")
        {
            isPlayerNearby = false;
            if (!isOpened && promptTextObject != null) promptTextObject.SetActive(false);
        }
    }
}