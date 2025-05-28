using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    public delegate void OnDeathDelegate();
    public event OnDeathDelegate OnDeath;
    public event System.Action OnDamaged;

    private void Start()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        OnDamaged?.Invoke(); // Notifica que se recibió daño

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();

            EnemyManager.Instance.UnregisterEnemy();
        }
    }

    public float GetCurrentHealth() => currentHealth;
}