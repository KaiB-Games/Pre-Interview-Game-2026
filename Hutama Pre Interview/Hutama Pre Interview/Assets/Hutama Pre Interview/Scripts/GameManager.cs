using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Saved Overworld State")]
    public Vector3 playerSavedPosition;
    public bool isReturningFromCombat = false;
    public bool isSkeletonDefeated = false;

    [Header("Progression Flow (1 to 5)")]
    public int currentCombatNumber = 1; // Tracks current combat stage (1st, 2nd... up to 5th Boss)

    [Header("Party Level & XP System")]
    public int partyLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;


    [Header("Persistent Player Vitality")]
    public int playerCurrentHP = -1;
    public int playerCurrentMP = -1;

    // Call this from your main menu / first load script so the player starts healthy!
    public void InitializeNewGameVitals(int maxHP, int maxMP)
    {
        playerCurrentHP = maxHP;
        playerCurrentMP = maxMP;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager successfully initialized and set to persist!");
        }
        else if (Instance != this)
        {
            Debug.Log("Duplicate GameManager detected and destroyed.");
            Destroy(gameObject);
        }
    }

    // Call this to add XP and cleanly compute multiple level ups
    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"Party gained {amount} XP! Current XP pool: {currentXP}/{xpToNextLevel}");

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            partyLevel++;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f); // Scales XP requirement for next level
            Debug.Log($"LEVEL UP! Party is now Level {partyLevel}! Next level requires {xpToNextLevel} XP.");
        }
    }

    // Call this when completing a combat encounter stage
    public void ProgressToNextEncounter()
    {
        currentCombatNumber++;
        if (currentCombatNumber > 5)
        {
            Debug.Log("Campaign Complete! The 5th Final Boss has been defeated!");
        }
    }

    [Header("Overworld Progression Maps")]
    [SerializeField] private List<string> overworldLevels = new List<string> { "Overworld", "Overworld 2", "Overworld 3" };

    // Helper method your combat manager can read to know where to send the player back to
    public string GetCurrentOverworldName()
    {
        // If your index goes out of bounds, default back to your first map safety net
        if (partyLevel - 1 >= overworldLevels.Count)
        {
            return overworldLevels[0];
        }

        // Grabs map based on encounter milestones or party tracking configurations
        return overworldLevels[partyLevel - 1];
    }
}