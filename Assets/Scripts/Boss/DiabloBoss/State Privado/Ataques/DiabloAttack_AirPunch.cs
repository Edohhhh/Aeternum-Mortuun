using UnityEngine;

public class DiabloAttack_AirPunch : IDiabloAttack
{
    private enum Phase
    {
        None,
        Warning,
        Falling,
        ImpactHold,
        Rising,
        Gap
    }

    public bool IsFinished { get; private set; }

    private DiabloController ctrl;
    private Phase phase = Phase.None;

    private int currentWave;
    private float timer;

    private Vector3 targetPos;           // posición del impacto
    private GameObject warnInstance;     // círculo de aviso
    private GameObject punchInstance;    // puño

    private float verticalVelocity;      // para caída con gravedad
    private float startPunchY;           // y de inicio del puño
    private float risenDistance;         // cuánto subió

    public void Start(DiabloController c)
    {
        ctrl = c;
        IsFinished = false;
        currentWave = 0;
        StartWave();

        ctrl.SpawnExtraEnemiesForAttack(1);
    }

    public void Tick(DiabloController c)
    {
        if (IsFinished || phase == Phase.None)
            return;

        float dt = Time.deltaTime;

        switch (phase)
        {
            case Phase.Warning:
                UpdateWarning(dt);
                break;

            case Phase.Falling:
                UpdateFalling(dt);
                break;

            case Phase.ImpactHold:
                UpdateImpactHold(dt);
                break;

            case Phase.Rising:
                UpdateRising(dt);
                break;

            case Phase.Gap:
                UpdateGap(dt);
                break;
        }
    }

    public void Stop(DiabloController c)
    {
        CleanupWave();
        IsFinished = true;
        phase = Phase.None;
    }

    // ================== Wave flow ==================

    private void StartWave()
    {
        CleanupWave();

        currentWave++;
        if (currentWave > ctrl.A6_Waves)
        {
            IsFinished = true;
            phase = Phase.None;
            return;
        }

        // empezamos fase de aviso
        phase = Phase.Warning;
        timer = 0f;

        // target inicial = posición actual del player
        Transform player = ctrl.Player;
        targetPos = player ? player.position : ctrl.transform.position;

        // Instanciar círculo de aviso si existe prefab
        if (ctrl.A6_TargetPrefab)
        {
            warnInstance = Object.Instantiate(
                ctrl.A6_TargetPrefab,
                targetPos,
                Quaternion.identity
            );

            // por si el prefab tiene sprite, lo ponemos tenue
            var sr = warnInstance.GetComponent<SpriteRenderer>();
            if (sr)
            {
                Color c = sr.color;
                c.a = Mathf.Min(c.a, 0.3f);
                sr.color = c;
            }
        }

        Debug.Log($"[DIABLO/AirPunch] Wave {currentWave} WARNING");
    }

    private void CleanupWave()
    {
        if (warnInstance)
            Object.Destroy(warnInstance);
        if (punchInstance)
            Object.Destroy(punchInstance);

        warnInstance = null;
        punchInstance = null;
    }

    // ================== Phases ==================

    private void UpdateWarning(float dt)
    {
        timer += dt;

        float warnTime = ctrl.A6_WarnTime;
        float sampleDelay = ctrl.A6_SampleDelay;

        // Hasta (warnTime - sampleDelay) seguimos al jugador
        float followTime = Mathf.Max(0f, warnTime - sampleDelay);
        if (timer < followTime)
        {
            Transform player = ctrl.Player;
            if (player)
            {
                targetPos = player.position;
                if (warnInstance)
                    warnInstance.transform.position = targetPos;
            }
        }

        // Fade de intensidad (si tiene sprite)
        if (warnInstance)
        {
            var sr = warnInstance.GetComponent<SpriteRenderer>();
            if (sr)
            {
                float t01 = Mathf.Clamp01(timer / warnTime);
                Color c = sr.color;
                c.a = Mathf.Lerp(0.25f, 0.85f, t01);
                sr.color = c;
            }
        }

        if (timer >= warnTime)
        {
            // pasamos a caída
            BeginFalling();
        }
    }

    private void BeginFalling()
    {
        phase = Phase.Falling;
        timer = 0f;

        // instanciamos el puño arriba del target
        if (ctrl.A6_PunchPrefab)
        {
            Vector3 startPos = targetPos + Vector3.up * ctrl.A6_FallHeight;

            punchInstance = Object.Instantiate(
                ctrl.A6_PunchPrefab,
                startPos,
                Quaternion.identity
            );

            startPunchY = startPos.y;
            risenDistance = 0f;
            verticalVelocity = 0f; // inicia en 0, se acelera por gravedad
        }

        // el círculo de aviso ya no es necesario
        if (warnInstance)
        {
            Object.Destroy(warnInstance);
            warnInstance = null;
        }

        Debug.Log($"[DIABLO/AirPunch] Wave {currentWave} FALLING");
    }

    private void UpdateFalling(float dt)
    {
        if (!punchInstance)
        {
            // si algo raro pasó, salteamos a impacto
            BeginImpact();
            return;
        }

        // caída con gravedad
        verticalVelocity -= ctrl.A6_Gravity * dt; // hacia abajo
        Vector3 pos = punchInstance.transform.position;
        pos.y += verticalVelocity * dt;

        // llegó o pasó el suelo (target)
        if (pos.y <= targetPos.y)
        {
            pos.y = targetPos.y;
            punchInstance.transform.position = pos;
            BeginImpact();
        }
        else
        {
            punchInstance.transform.position = pos;
        }
    }

    private void BeginImpact()
    {
        phase = Phase.ImpactHold;
        timer = 0f;

        // daño instantáneo
        DoImpactDamage();

        Debug.Log($"[DIABLO/AirPunch] Wave {currentWave} IMPACT");
    }

    private void UpdateImpactHold(float dt)
    {
        timer += dt;
        if (timer >= ctrl.A6_ImpactHoldTime)
        {
            BeginRising();
        }
    }

    private void BeginRising()
    {
        phase = Phase.Rising;
        timer = 0f;
        risenDistance = 0f;
        verticalVelocity = 0f; // ya no usamos gravedad, usamos velocidad constante
        Debug.Log($"[DIABLO/AirPunch] Wave {currentWave} RISING");
    }

    private void UpdateRising(float dt)
    {
        if (!punchInstance)
        {
            // pasamos directo al gap
            BeginGap();
            return;
        }

        float riseSpeed = ctrl.A6_RiseSpeed;
        Vector3 pos = punchInstance.transform.position;
        float delta = riseSpeed * dt;
        pos.y += delta;
        punchInstance.transform.position = pos;
        risenDistance += delta;

        if (risenDistance >= ctrl.A6_RiseDistance)
        {
            // ya subió suficiente, destruimos puño y pasamos al gap
            Object.Destroy(punchInstance);
            punchInstance = null;
            BeginGap();
        }
    }

    private void BeginGap()
    {
        phase = Phase.Gap;
        timer = 0f;
        Debug.Log($"[DIABLO/AirPunch] Wave {currentWave} GAP");
    }

    private void UpdateGap(float dt)
    {
        timer += dt;
        if (timer >= ctrl.A6_WaveGap)
        {
            StartWave();
        }
    }

    // ================== Damage ==================

    private void DoImpactDamage()
    {
        float radius = ctrl.A6_DamageRadius;
        LayerMask mask = ctrl.A6_PlayerMask;

        Collider2D hit = Physics2D.OverlapCircle(targetPos, radius, mask);
        if (hit)
        {
            var health = hit.GetComponent<PlayerHealth>() ??
                         hit.GetComponentInParent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(ctrl.A6_Damage, targetPos);
                Debug.Log($"[DIABLO/AirPunch] HIT player for {ctrl.A6_Damage}");
            }
        }
    }
}
