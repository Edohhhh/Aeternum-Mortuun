using UnityEngine;


public class MoleBulletExplosive : MonoBehaviour
{
    private Vector2 dir;
    private float speed;
    private GameObject childPrefab;
    private float childSpeed;
    private float childDamage;
    private LayerMask playerMask;

    [SerializeField] private float life = 1.2f; // corta para forzar la explosión

    public void Initialize(Vector2 direction, float spd, GameObject child, float childSpd, float childDmg, LayerMask mask)
    {
        dir = direction.normalized; speed = spd;
        childPrefab = child; childSpeed = childSpd; childDamage = childDmg; playerMask = mask;
    }

    private void Update()
    {
        transform.position += (Vector3)(dir * speed * Time.deltaTime);
        life -= Time.deltaTime;
        if (life <= 0f) { Explode(); Destroy(gameObject); }
    }

    private void Explode()
    {
        if (!childPrefab) return;
        SpawnChild(Vector2.up);
        SpawnChild(Vector2.down);
        SpawnChild(Vector2.left);
        SpawnChild(Vector2.right);
    }

    private void SpawnChild(Vector2 d)
    {
        var go = Instantiate(childPrefab, transform.position, Quaternion.identity);
        var cb = go.GetComponent<MoleBulletChild>();
        if (cb) cb.Initialize(d, childSpeed, childDamage, playerMask);
    }
}
