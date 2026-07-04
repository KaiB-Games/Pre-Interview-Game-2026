using UnityEngine;

public class OverworldSetup : MonoBehaviour
{
    [SerializeField] private GameObject player;

    //Array for handling multiple enemies
    [SerializeField] private GameObject[] skeletonEnemies;

    void Start()
    {
        if (GameManager.Instance == null) return;

        //Destorys Enemy Sprite
        if (GameManager.Instance.isSkeletonDefeated)
        {
            foreach (GameObject enemy in skeletonEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }
        }

        //Player Reset
        if (GameManager.Instance.isReturningFromCombat)
        {
            Vector3 spawnPosition = GameManager.Instance.playerSavedPosition;
            spawnPosition.y += 0.5f;

            player.transform.position = spawnPosition;

           
            var vcam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
            if (vcam != null)
            {
                vcam.ForceCameraPosition(player.transform.position, Quaternion.identity);
            }

            GameManager.Instance.isReturningFromCombat = false;
        }
    }
}