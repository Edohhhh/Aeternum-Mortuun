using UnityEngine;

public class MoleBulletStraight : MonoBehaviour
{
    private Vector2 dir;
    private float speed;
    private float damage;
    private LayerMask playerMask;

    [SerializeField] private float life = 3f;

    public void Initialize(Vector2 direction, float spd, float dmg, LayerMask mask)
    {
        dir = direction.normalized; speed = spd; damage = dmg; playerMask = mask;
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