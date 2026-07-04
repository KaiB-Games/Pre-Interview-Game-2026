using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [SerializeField] private string combatSceneName = "CombatScene";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerSavedPosition = collision.transform.position;
            }

            Debug.Log("Encounter triggered! Transitioning to battle...");
            SceneManager.LoadScene(combatSceneName);
        }
    }
}