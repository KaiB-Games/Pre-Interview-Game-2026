using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float lifetime = 0.3f; 
    void Start() => Destroy(gameObject, lifetime);
}