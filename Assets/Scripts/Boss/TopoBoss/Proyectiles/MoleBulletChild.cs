using UnityEngine;

public class MoleBulletChild : MonoBehaviour
{
    private Vector2 dir;
    private float speed;
    private float damage;
    private LayerMask playerMask;

    [SerializeField] private float life = 2.2f;

    public void Initialize(Vector2 d, float spd, float dmg, LayerMask mask)
    {
        dir = d.normalized; speed = spd; damage = dmg; playerMask = mask;
    }

    private void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerMask) != 0)
        {
            var hp = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
            if (hp) hp.TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
    }
}

