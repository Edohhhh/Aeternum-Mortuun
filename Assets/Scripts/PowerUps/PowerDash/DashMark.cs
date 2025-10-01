using UnityEngine;

public class DashMark : MonoBehaviour
{
    public float duration = 3f;
    public float shieldDuration = 3f;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var shield = other.GetComponent<ShieldEffect>();
            if (shield == null)
            {
                shield = other.gameObject.AddComponent<ShieldEffect>();
            }

            shield.ActivateShield(shieldDuration);
            Destroy(gameObject);
        }
    }
}
