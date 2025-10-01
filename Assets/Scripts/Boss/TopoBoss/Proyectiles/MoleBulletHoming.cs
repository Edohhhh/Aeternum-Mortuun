using UnityEngine;

public class MoleBulletHoming : MonoBehaviour
{
    private Transform target;

    // Velocidad objetivo (la que te pasa el spawner) y velocidad actual con aceleraci�n
    private float maxSpeed;
    private float currentSpeed;

    // Giro objetivo (turn rate) y giro actual con aceleraci�n angular
    private float targetTurnDegPerSec;
    private float currentTurnDegPerSec;

    private float damage;
    private LayerMask playerMask;

    [SerializeField] private float life = 3.5f;

    [Header("Aceleraciones")]
    [Tooltip("m/s�: qu� tan r�pido llega a su velocidad m�xima")]
    [SerializeField] private float accelPerSec = 18f;

    [Tooltip("Fracci�n de la velocidad m�xima con la que arranca (0..1)")]
    [SerializeField, Range(0f, 1f)] private float startSpeedFactor = 0.30f;

    [Tooltip("deg/s�: qu� tan r�pido alcanza su turn rate objetivo")]
    [SerializeField] private float turnAccelDegPerSec2 = 900f;

    [Tooltip("Turn rate m�nimo de arranque (deg/s) para que no sea totalmente recto")]
    [SerializeField] private float minStartTurnDegPerSec = 90f;

    public void Initialize(Transform tgt, float spd, float turnDeg, float dmg, LayerMask mask)
    {
        target = tgt;
        maxSpeed = spd;
        targetTurnDegPerSec = turnDeg;
        damage = dmg;
        playerMask = mask;

        // Arranque suave
        currentSpeed = Mathf.Max(0.01f, maxSpeed * startSpeedFactor);
        currentTurnDegPerSec = Mathf.Max(minStartTurnDegPerSec, targetTurnDegPerSec * startSpeedFactor);
    }

    private void Update()
    {
        life -= Time.deltaTime;
        if (life <= 0f) { Destroy(gameObject); return; }

        // 1) Ramp de velocidad hacia la m�xima
        currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, accelPerSec * Time.deltaTime);

        // 2) Ramp de giro hacia el turn rate objetivo
        currentTurnDegPerSec = Mathf.MoveTowards(
            currentTurnDegPerSec,
            targetTurnDegPerSec,
            turnAccelDegPerSec2 * Time.deltaTime
        );

        // 3) Steering con l�mite de giro por frame
        Vector2 forward = (Vector2)transform.right;
        Vector2 desired = forward;

        if (target)
        {
            Vector2 to = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float maxRad = currentTurnDegPerSec * Mathf.Deg2Rad * Time.deltaTime;
            float angle = Vector2.SignedAngle(forward, to);
            float clamped = Mathf.Clamp(angle * Mathf.Deg2Rad, -maxRad, maxRad);
            transform.Rotate(0f, 0f, clamped * Mathf.Rad2Deg);
            desired = transform.right;
        }

        // 4) Movimiento con la velocidad actual (acelerada)
        transform.position += (Vector3)(desired * currentSpeed * Time.deltaTime);
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
