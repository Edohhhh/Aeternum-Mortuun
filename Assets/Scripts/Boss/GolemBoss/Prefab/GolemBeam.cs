using UnityEngine;
using System.Collections.Generic;

public class GolemBeam : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] float duration = 3f;
    [SerializeField] float damagePerTick = 1f;
    [SerializeField] float tickInterval = 0.25f;
    [SerializeField] float maxDistance = 12f;
    [SerializeField] float thickness = 0.6f;
    [SerializeField] float knockbackImpulse = 6f;
    [SerializeField] LayerMask obstacleMask, playerMask;

    [Header("Tracking (delay + giro)")]
    [SerializeField] float followDelay = 0.5f;   // apunta a donde estaba hace X s
    [SerializeField] float historyWindow = 2f;   // > followDelay
    [SerializeField] float turnRateDegPerSec = 120f;

    [Header("Visual")]
    [SerializeField] LineRenderer line;
    [SerializeField] Material lineMaterial;      // opcional (si null, se usa Sprites/Default)
    [SerializeField] float pulseSpeed = 5f, pulseIntensity = 0.2f;

    [Header("Sorting")]
    [SerializeField] string sortingLayerName = "Effects";
    [SerializeField] int sortingOrder = 100;

    Transform owner, target;
    float life, tick, baseWidth;
    bool active;
    Vector2 aimDir = Vector2.right;

    struct Sample { public float t; public Vector2 p; public Sample(float t, Vector2 p) { this.t = t; this.p = p; } }
    readonly List<Sample> samples = new List<Sample>(128);

    public void Initialize(Transform owner, Transform target, float durationOverride,
                           float dmgPerTick, float interval, float range,
                           float width, float knockback, LayerMask playerMask, LayerMask obstacleMask)
    {
        this.owner = owner; this.target = target;
        duration = durationOverride; damagePerTick = dmgPerTick; tickInterval = interval;
        maxDistance = range; thickness = width; knockbackImpulse = knockback;
        this.playerMask = playerMask; this.obstacleMask = obstacleMask;
        Setup();
    }

    void Awake()
    {
        if (!line) line = gameObject.AddComponent<LineRenderer>();
        Setup();
    }

    void Setup()
    {
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = line.endWidth = thickness;
        line.alignment = LineAlignment.TransformZ;
        line.numCapVertices = 6; line.numCornerVertices = 3;
        line.textureMode = LineTextureMode.Stretch;

        if (lineMaterial) line.material = lineMaterial;
        else
        {
            var mat = new Material(Shader.Find("Sprites/Default")) { renderQueue = 3000 };
            line.material = mat;
        }
        line.sortingLayerName = sortingLayerName;
        line.sortingOrder = sortingOrder;

        baseWidth = thickness;
        life = duration; tick = 0f; active = true; line.enabled = true;

        if (owner && target)
        {
            var d = (Vector2)target.position - (Vector2)owner.position;
            if (d.sqrMagnitude > 0.0001f) aimDir = d.normalized;
        }

        samples.Clear();
        if (target) samples.Add(new Sample(Time.time, target.position));
    }

    void Update()
    {
        if (!active) return;
        if (!owner || !target) { Kill(); return; }

        float dt = Time.deltaTime, now = Time.time;
        life -= dt; tick += dt;

        samples.Add(new Sample(now, target.position));
        float oldest = now - Mathf.Max(historyWindow, followDelay) - 0.1f;
        while (samples.Count > 1 && samples[0].t < oldest) samples.RemoveAt(0);

        Vector2 delayedPos = (followDelay > 0.001f) ? GetDelayedPosition(now - followDelay) : (Vector2)target.position;

        Vector2 origin = owner.position;
        Vector2 desiredDir = delayedPos - origin;
        desiredDir = (desiredDir.sqrMagnitude > 0.0001f) ? desiredDir.normalized : aimDir;

        float maxRad = turnRateDegPerSec * Mathf.Deg2Rad * dt;
        Vector3 rot3 = Vector3.RotateTowards(new Vector3(aimDir.x, aimDir.y, 0f),
                                             new Vector3(desiredDir.x, desiredDir.y, 0f),
                                             maxRad, 0f);
        aimDir = new Vector2(rot3.x, rot3.y).normalized;

        float pulse = Mathf.Sin(now * pulseSpeed) * pulseIntensity + 1f;
        line.startWidth = line.endWidth = baseWidth * pulse;

        float length = maxDistance;
        var hit = Physics2D.Raycast(origin, aimDir, maxDistance, obstacleMask);
        if (hit.collider) length = hit.distance;

        Vector2 end = origin + aimDir * length;
        line.SetPosition(0, origin);
        line.SetPosition(1, end);

        if (tick >= tickInterval) { tick = 0f; DoDamage(origin, aimDir, length); }
        if (life <= 0f) Kill();
    }

    Vector2 GetDelayedPosition(float t)
    {
        int n = samples.Count;
        if (n == 0) return target ? (Vector2)target.position : Vector2.zero;
        if (n == 1 || t <= samples[0].t) return samples[0].p;
        if (t >= samples[n - 1].t) return samples[n - 1].p;

        int lo = 0, hi = n - 1;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (samples[mid].t < t) lo = mid + 1; else hi = mid;
        }
        int i1 = Mathf.Clamp(lo, 1, n - 1), i0 = i1 - 1;
        float t0 = samples[i0].t, t1 = samples[i1].t, a = Mathf.InverseLerp(t0, t1, t);
        return Vector2.LerpUnclamped(samples[i0].p, samples[i1].p, a);
    }

    void DoDamage(Vector2 origin, Vector2 dir, float length)
    {
        Vector2 center = origin + dir * (length * 0.5f);
        var hits = Physics2D.OverlapBoxAll(center, new Vector2(length, thickness * 1.2f),
                                           Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, playerMask);
        foreach (var h in hits)
        {
            var hp = h.GetComponent<PlayerHealth>() ?? h.GetComponentInParent<PlayerHealth>();
            if (!hp) continue;
            hp.TakeDamage(damagePerTick, center);
            var rb = h.attachedRigidbody;
            if (rb && knockbackImpulse > 0f) rb.AddForce(dir * knockbackImpulse, ForceMode2D.Impulse);
        }
    }

    void Kill()
    {
        active = false;
        if (line) line.enabled = false;
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (!owner) return;
        Vector2 origin = owner.position, dir = (aimDir.sqrMagnitude > 0.0001f ? aimDir : Vector2.right);
        float length = maxDistance, ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(origin + dir * (length * 0.5f), Quaternion.Euler(0, 0, ang), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector2(length, thickness * 1.2f));
        Gizmos.matrix = Matrix4x4.identity;
    }
}
