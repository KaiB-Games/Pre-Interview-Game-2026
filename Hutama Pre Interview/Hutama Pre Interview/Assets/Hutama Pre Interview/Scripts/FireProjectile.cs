using UnityEngine;

public class FireProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed = 12f;
    private bool targetFound = false;

    // This method now perfectly accepts the parameters sent by EnemyBehaviour
    public void InitializeDrop(Vector3 playerPos, float fallSpeed)
    {
        targetPosition = playerPos;
        speed = fallSpeed;
        targetFound = true;
    }

    void Update()
    {
        if (!targetFound) return;

        // Fly directly towards the player's saved position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Self-destruct when close to the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.4f)
        {
            Destroy(gameObject);
        }
    }
}