using UnityEngine;

public class GolemBeam : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float duration = 3f;          // dura 3s
    [SerializeField] private float damagePerTick = 1f;     // daño por tick
    [SerializeField] private float tickInterval = 0.25f;   // frecuencia de daño
    [SerializeField] private float maxDistance = 12f;      // alcance
    [SerializeField] private float thickness = 0.6f;       // grosor "visual" y de colisión
    [SerializeField] private float knockbackImpulse = 6f;  // 0 = sin knockback
    [SerializeField] private LayerMask obstacleMask;       // muros
    [SerializeField] private LayerMask playerMask;         // capa del Player

    [Header("Visual")]
    [SerializeField] private LineRenderer line;            // opcional: si es null se crea uno
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float pulseIntensity = 0.2f;

    private Transform owner;     // gólem
    private Transform target;    // jugador
    private float life;
    private float tick;
    private float baseWidth;
    private bool active;

    public void Initialize(Transform owner, Transform target, float durationOverride,
                           float dmgPerTick, float interval, float range,
                           float width, float knockback, LayerMask playerMask, LayerMask obstacleMask)
    {
        this.owner = owner;
        this.target = target;
        duration = durationOverride;
        damagePerTick = dmgPerTick;
        tickInterval = interval;
        maxDistance = range;
        thickness = width;
        knockbackImpulse = knockback;
        this.playerMask = playerMask;
        this.obstacleMask = obstacleMask;

        Setup();
    }

    private void Awake()
    {
        // Si se instancia sin Initialize, igual queda funcional con defaults
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        Setup();
    }

    private void Setup()
    {
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = thickness;
        line.endWidth = thickness;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.alignment = LineAlignment.TransformZ; // se ve bien en 2D
        baseWidth = thickness;

        life = duration;
        tick = 0f;
        active = true;
        line.enabled = true;
    }

    private void Update()
    {
        if (!active) return;

        // matar si perdimos owner o target
        if (owner == null || target == null) { Kill(); return; }

        life -= Time.deltaTime;
        tick += Time.deltaTime;

        // “latido” visual
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity + 1f;
        line.startWidth = baseWidth * pulse;
        line.endWidth = baseWidth * pulse;

        // actualizar rayo (posiciones y fin contra muros)
        Vector2 origin = owner.position;
        Vector2 dir = ((Vector2)target.position - origin).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        float length = maxDistance;
        var hitWall = Physics2D.Raycast(origin, dir, maxDistance, obstacleMask);
        if (hitWall.collider != null) length = hitWall.distance;

        Vector2 end = origin + dir * length;
        line.SetPosition(0, origin);
        line.SetPosition(1, end);

        // aplicar daño por tick
        if (tick >= tickInterval)
        {
            tick = 0f;
            DoDamage(origin, dir, length);
        }

        if (life <= 0f) Kill();
    }

    private void DoDamage(Vector2 origin, Vector2 dir, float length)
    {
        // usamos un OverlapBox "acostado" sobre el rayo para chequear al Player
        Vector2 size = new Vector2(length, thickness * 1.2f);
        Vector2 center = origin + dir * (length * 0.5f);
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        var hits = Physics2D.OverlapBoxAll(center, size, angleDeg, playerMask);
        foreach (var h in hits)
        {
            var hp = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damagePerTick, center); // tu firma usada en EnemyAttack
                var prb = h.attachedRigidbody;
                if (prb != null && knockbackImpulse > 0f)
                    prb.AddForce(dir * knockbackImpulse, ForceMode2D.Impulse);
            }
        }
    }

    private void Kill()
    {
        active = false;
        if (line != null) line.enabled = false;
        Destroy(gameObject);
    }

    // debug del área de daño
    private void OnDrawGizmosSelected()
    {
        if (!owner || !target) return;
        Vector2 origin = owner.position;
        Vector2 dir = ((Vector2)target.position - origin).normalized;
        float length = maxDistance;
        Gizmos.color = Color.red;
        Vector2 center = origin + dir * (length * 0.5f);
        Vector2 size = new Vector2(length, thickness * 1.2f);
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Matrix4x4 rot = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angleDeg), Vector3.one);
        Gizmos.matrix = rot;
        Gizmos.DrawWireCube(Vector3.zero, size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
