using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Saved Overworld State")]
    public Vector3 playerSavedPosition;
    public bool isReturningFromCombat = false;
    public bool isSkeletonDefeated = false;

    void Awake()
    {
        if (Instance == null)
        {
            //Make sure GameManager doesnt get destroyed
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager successfully initialized and set to persist!");
        }
        else if (Instance != this)
        {
            //Destroys Duplicate GameManager after it loads overworld
            Debug.Log("Duplicate GameManager detected and destroyed.");
            Destroy(gameObject);
        }
    }
}