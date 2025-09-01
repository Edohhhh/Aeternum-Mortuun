using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float delay = 0.1f;
    void Start() => Destroy(gameObject, delay);
}
