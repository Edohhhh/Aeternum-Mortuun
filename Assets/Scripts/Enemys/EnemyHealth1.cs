using UnityEngine;

public class EnemyHealth1 : MonoBehaviour
{
    [SerializeField] public int health = 30;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(name + " took " + damage + " damage.");

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}

