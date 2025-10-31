using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MimicBulletStraight : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float damage = 1f;
    [SerializeField] private LayerMask playerMask;

    private Vector2 dir;

    public void Init(Vector2 direction, float spd, float dmg, LayerMask mask)
    {
        dir = direction.normalized;
        speed = spd;
        damage = dmg;
        playerMask = mask;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerMask) == 0) return;

        var h = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (h != null) h.TakeDamage(damage, transform.position);

        Destroy(gameObject);
    }
}