using UnityEngine;

public class DashTrap : MonoBehaviour
{
    [Header("Configuración")]
    public float stunDuration = 2f;
    public float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<EnemyHealth>();
            if (health != null && other.GetComponent<StunEffect>() == null)
            {
                other.gameObject.AddComponent<StunEffect>().Initialize(stunDuration);
                Destroy(gameObject); // solo se activa una vez
            }
        }
    }
}
