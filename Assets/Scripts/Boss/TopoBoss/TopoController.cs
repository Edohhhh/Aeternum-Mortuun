using UnityEngine;
using UnityEngine.Serialization;

public class TopoController : MonoBehaviour, IEnemyDataProvider
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Waypoints (aparecer)")]
    [SerializeField] private Transform[] waypoints;

    [Header("Stats base (visibles)")]
    [SerializeField] private float detectionRadius = 7f;
    [SerializeField] private float baseMaxSpeed = 2.0f;
    [SerializeField] private float baseAcceleration = 12f;

    [Header("Fases (por % de vida)")]
    [SerializeField, Range(0, 1)] private float phase1Threshold = 0.67f;
    [SerializeField, Range(0, 1)] private float phase2Threshold = 0.34f;

    [Header("Velocidad bajo tierra (por fase)")]
    [SerializeField] private float undergroundSpeedPhase1 = 7f;
    [SerializeField] private float undergroundSpeedPhase2 = 11f;
    [SerializeField] private float undergroundSpeedPhase3 = 16f;

    [Header("Ataque (del patrón por fase)")]
    [SerializeField] private float attackDelayPhase1 = 0.9f;
    [SerializeField] private float attackDelayPhase2 = 0.65f;
    [SerializeField] private float attackDelayPhase3 = 0.45f;

    [SerializeField] private int volleysPhase1 = 1;
    [SerializeField] private int volleysPhase2 = 2;
    [SerializeField] private int volleysPhase3 = 3;

    [Header("Proyectiles prefabs")]
    [SerializeField] private GameObject straightBulletPrefab;
    [SerializeField] private GameObject homingBulletPrefab;
    [SerializeField] private GameObject explosiveBulletPrefab;
    [SerializeField] private GameObject childBulletPrefab;

    [Header("Proyectiles – Tuning global")]
    [SerializeField] private float straightSpeed = 9f;
    [SerializeField] private float homingSpeed = 7.5f;
    [SerializeField] private float homingTurnDegPerSec = 360f;
    [SerializeField] private float explosiveSpeed = 8f;
    [SerializeField] private float childSpeed = 5f;

    [Header("Daños")]
    [SerializeField] private float straightDamage = 1f;
    [SerializeField] private float homingDamage = 1f;
    [SerializeField] private float childDamage = 1f;

    // === Fase: FX & Multipliers ===
    [Header("Phase FX & Multipliers")]
    [SerializeField] private float animSpeedPhase1 = 1.00f;
    [SerializeField] private float animSpeedPhase2 = 1.15f;
    [SerializeField] private float animSpeedPhase3 = 1.30f;

    [SerializeField] private float projectileSpeedMultPhase1 = 1.00f;
    [SerializeField] private float projectileSpeedMultPhase2 = 1.25f;
    [SerializeField] private float projectileSpeedMultPhase3 = 1.50f;

    private int currentPhase = -1;
    private float currentProjMult = 1f;
    public float GetProjectileSpeedMult() => currentProjMult;

    [Header("LayerMasks")]
    [SerializeField] private LayerMask playerMask;

    // Estado interno
    private float maxHealthAtSpawn = 15f;
    private bool isUnderground = false;
    private Coroutine travelCR;

    // Waypoint: no repetir el último
    private int lastWaypointIndex = -1;
    private int pendingWaypointIndex = -1;

    // FSM & comps
    private FSM<EnemyInputs> fsm;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private EnemyHealth health;

    // Ref al estado de ataque (para disparar desde Animation Events)
    private TopoAttackState _attackRef;
    public void RegisterAttackState(TopoAttackState s) => _attackRef = s;

    // Exposición para estados
    public Transform Player => player;
    public Transform[] Waypoints => waypoints;
    public Animator Anim => animator;
    public Rigidbody2D Body => rb;
    public SpriteRenderer SR => sr;

    // Prefabs/tuning getters para AttackState
    public GameObject StraightPrefab => straightBulletPrefab;
    public GameObject HomingPrefab => homingBulletPrefab;
    public GameObject ExplosivePrefab => explosiveBulletPrefab;
    public GameObject ChildPrefab => childBulletPrefab;

    public float StraightSpeed => straightSpeed;
    public float HomingSpeed => homingSpeed;
    public float HomingTurn => homingTurnDegPerSec;
    public float ExplosiveSpeed => explosiveSpeed;
    public float ChildSpeed => childSpeed;

    public float StraightDamage => straightDamage;
    public float HomingDamage => homingDamage;
    public float ChildDamage => childDamage;

    public LayerMask PlayerMask => playerMask;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.gravityScale = 0f; // sin gravedad
            // NO empujar: congelar posición y rotación
            rb.constraints = RigidbodyConstraints2D.FreezePositionX
                           | RigidbodyConstraints2D.FreezePositionY
                           | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnEnable()
    {
        if (rb) rb.gravityScale = 0f; // blindaje por si un clip lo pisa
    }

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();

        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();

        if (health != null)
        {
            health.OnDeath += () => Transition(EnemyInputs.Die);
            maxHealthAtSpawn = Mathf.Max(health.GetCurrentHealth(), 1f);
        }

        // FSM
        var idle = new TopoIdleState(this);   // Idle "pasivo" (no dispara nada solo)
        var attack = new TopoAttackState(this); // event-driven
        var death = new EnemyDeathState(this);

        fsm = new FSM<EnemyInputs>(idle);

        // Solo transiciones esenciales
        idle.AddTransition(EnemyInputs.SpecialAttack, attack);
        idle.AddTransition(EnemyInputs.Die, death);
        attack.AddTransition(EnemyInputs.SeePlayer, idle); // la usamos como "volver a Idle" desde AE_OnAttackEnd

        // Arrancar el ciclo un frame después (evita doble Emerge)
        currentPhase = GetPhase();
        ApplyPhaseTuning(currentPhase);
        StartCoroutine(_StartCycleNextFrame());
    }

    private System.Collections.IEnumerator _StartCycleNextFrame()
    {
        yield return null; // Animator listo
        if (animator)
        {
            animator.ResetTrigger("Emerge");
            animator.SetTrigger("Emerge");
        }
    }

    private void Update()
    {
        fsm.Update();


        int p = GetPhase();
        if (p != currentPhase)
        {
            currentPhase = p;
            ApplyPhaseTuning(p);
        }

        // No empujar "ver jugador" – el ciclo lo llevan los Animation Events.
        // Solo flip visual si está visible
        if (player && sr && sr.enabled)
        {
            Vector2 dir = player.position - transform.position;
            sr.flipX = dir.x < 0;
        }
    }

    public void Transition(EnemyInputs input) => fsm?.Transition(input);

    // ------------------- ANIMATION EVENTS -------------------
    public void AE_OnEmergeEnd()
    {
        // Termina Emerge  entrar a Attack (estado  anim)
        if (animator) animator.SetTrigger("Attack");
        Transition(EnemyInputs.SpecialAttack);
    }

    public void AE_OnAttackFire()
    {
        _attackRef?.FireVolleyByPhase(); // dispara patrón según fase
    }

    public void AE_OnAttackEnd()
    {
        // Termina Attack  volver a Idle (FSM) y disparar Burrow (anim)
        Transition(EnemyInputs.SeePlayer); // usamos esta transición como "volver a Idle"
        if (animator) animator.SetTrigger("Burrow");
    }

    public void AE_OnBurrowHide() // opcional: ocultar justo en ese frame
    {
        if (sr) sr.enabled = false;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        isUnderground = true;
    }

    public void AE_OnBurrowEnd()
    {
        // Por si no llamaste OnBurrowHide
        if (sr) sr.enabled = false;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        isUnderground = true;

        if (travelCR != null) StopCoroutine(travelCR);
        travelCR = StartCoroutine(UndergroundTravelToRandomWaypoint());
    }

    // ------------------- VIAJE BAJO TIERRA -------------------
    private System.Collections.IEnumerator UndergroundTravelToRandomWaypoint()
    {
        if (rb) rb.linearVelocity = Vector2.zero;

        if (waypoints == null || waypoints.Length == 0)
        {
            FinishUndergroundTravel(transform.position);
            yield break;
        }

        Transform target = GetRandomWaypointNotLast();
        if (!target)
        {
            FinishUndergroundTravel(transform.position);
            yield break;
        }

        pendingWaypointIndex = IndexOfWaypoint(target);

        float dist = Vector2.Distance(transform.position, target.position);
        float speedUG = Mathf.Max(0.01f, GetUndergroundSpeedByPhase());
        float travelTime = dist / speedUG;

        yield return new WaitForSeconds(travelTime);

        FinishUndergroundTravel(target.position);
    }

    private void FinishUndergroundTravel(Vector3 arrivePos)
    {
        // Teleport al destino y emerger
        transform.position = new Vector3(arrivePos.x, arrivePos.y, transform.position.z);

        if (sr) sr.enabled = true;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = true;

        isUnderground = false;

        lastWaypointIndex = pendingWaypointIndex;
        pendingWaypointIndex = -1;

        if (animator) animator.SetTrigger("Emerge");
    }

    // --------- Helpers de fase/velocidad ----------
    public float GetHealthRatio()
    {
        if (!health) return 1f;
        float cur = Mathf.Max(0f, health.GetCurrentHealth());
        return Mathf.Clamp01(cur / Mathf.Max(0.0001f, maxHealthAtSpawn));
    }

    public int GetPhase()
    {
        float r = GetHealthRatio();
        if (r >= phase1Threshold) return 1;
        if (r >= phase2Threshold) return 2;
        return 3;
    }

    public float GetUndergroundSpeedByPhase()
    {
        switch (GetPhase())
        {
            case 1: return undergroundSpeedPhase1;
            case 2: return undergroundSpeedPhase2;
            default: return undergroundSpeedPhase3;
        }
    }

    public float GetAttackDelayByPhase()
    {
        switch (GetPhase())
        {
            case 1: return attackDelayPhase1;
            case 2: return attackDelayPhase2;
            default: return attackDelayPhase3;
        }
    }

    public int GetVolleysByPhase()
    {
        switch (GetPhase())
        {
            case 1: return volleysPhase1;
            case 2: return volleysPhase2;
            default: return volleysPhase3;
        }
    }

    // --------- Waypoints helpers ----------
    private int IndexOfWaypoint(Transform t)
    {
        if (waypoints == null) return -1;
        for (int i = 0; i < waypoints.Length; i++)
            if (waypoints[i] == t) return i;
        return -1;
    }

    // Aleatorio distinto al último (si hay 1)
    private Transform GetRandomWaypointNotLast()
    {
        if (waypoints == null || waypoints.Length == 0) return null;

        var valid = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < waypoints.Length; i++)
        {
            var w = waypoints[i];
            if (!w) continue;
            if (i == lastWaypointIndex && waypoints.Length > 1) continue;
            valid.Add(w);
        }

        if (valid.Count == 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
                if (waypoints[i]) return waypoints[i];
            return null;
        }

        return valid[Random.Range(0, valid.Count)];
    }

    private void ApplyPhaseTuning(int phase)
    {
        // Animator speed (afecta Emerge/Attack/Burrow → sensación de urgencia)
        if (animator)
        {
            switch (phase)
            {
                case 1: animator.speed = animSpeedPhase1; break;
                case 2: animator.speed = animSpeedPhase2; break;
                default: animator.speed = animSpeedPhase3; break;
            }
        }

        // Multiplicador de velocidad de balas
        switch (phase)
        {
            case 1: currentProjMult = projectileSpeedMultPhase1; break;
            case 2: currentProjMult = projectileSpeedMultPhase2; break;
            default: currentProjMult = projectileSpeedMultPhase3; break;
        }

        // (Opcional) FX: flash, shake, sonido, etc.
        // Debug.Log($"[TOPO] Phase {phase} -> animSpeed={animator?.speed}, projMult={currentProjMult}");
    }

    // --------- IEnemyDataProvider ----------
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => detectionRadius;
    public float GetDamage() => 0f; // no melee
    public float GetMaxSpeed() => baseMaxSpeed;
    public float GetAcceleration() => baseAcceleration;
    public float GetCurrentHealth() => health ? health.GetCurrentHealth() : 0f;
}

//using UnityEngine;
//using UnityEngine.Serialization;

//public class TopoController : MonoBehaviour, IEnemyDataProvider
//{
//    [Header("References")]
//    [SerializeField] private Transform player;

//    [Header("Waypoints (aparecer)")]
//    [SerializeField] private Transform[] waypoints;

//    [Header("Stats base (visibles)")]
//    [SerializeField] private float detectionRadius = 7f;
//    [SerializeField] private float baseMaxSpeed = 2.0f;      
//    [SerializeField] private float baseAcceleration = 12f;

//    [Header("Fases (por % de vida)")]
//    [SerializeField, Range(0, 1)] private float phase1Threshold = 0.67f;
//    [SerializeField, Range(0, 1)] private float phase2Threshold = 0.34f;

//    [Header("Tuning por fase")]
//    [SerializeField] private float ugSpeedPhase1 = 7f;   
//    [SerializeField] private float ugSpeedPhase2 = 11f;
//    [SerializeField] private float ugSpeedPhase3 = 16f;

//    [SerializeField] private float attackDelayPhase1 = 0.9f;   
//    [SerializeField] private float attackDelayPhase2 = 0.65f;
//    [SerializeField] private float attackDelayPhase3 = 0.45f;

//    [SerializeField] private int volleysPhase1 = 1;  
//    [SerializeField] private int volleysPhase2 = 2;
//    [SerializeField] private int volleysPhase3 = 3;

//    [Header("Proyectiles prefabs")]
//    [SerializeField] private GameObject straightBulletPrefab;     
//    [SerializeField] private GameObject homingBulletPrefab;       
//    [SerializeField] private GameObject explosiveBulletPrefab;    
//    [SerializeField] private GameObject childBulletPrefab;        

//    [Header("Proyectiles – Tuning global")]
//    [SerializeField] private float straightSpeed = 9f;
//    [SerializeField] private float homingSpeed = 7.5f;
//    [SerializeField] private float homingTurnDegPerSec = 360f;
//    [SerializeField] private float explosiveSpeed = 8f;
//    [SerializeField] private float childSpeed = 5f;

//    [Header("Daños")]
//    [SerializeField] private float straightDamage = 1f;
//    [SerializeField] private float homingDamage = 1f;
//    [SerializeField] private float childDamage = 1f;

//    // --- CAMPOS NUEVOS ---
//    [SerializeField] private float undergroundSpeedPhase1 = 7f;
//    [SerializeField] private float undergroundSpeedPhase2 = 11f;
//    [SerializeField] private float undergroundSpeedPhase3 = 16f;

//    // Guarda el último waypoint usado y el próximo (mientras viaja)
//    private int lastWaypointIndex = -1;
//    private int pendingWaypointIndex = -1;


//    [Header("LayerMasks")]
//    [SerializeField] private LayerMask playerMask;

//    private float maxHealthAtSpawn = 15f;
//    private bool isUnderground = false;
//    private Coroutine travelCR;

//    private TopoAttackState _attackRef; // lo setea el estado en Awake

//    public void RegisterAttackState(TopoAttackState s) => _attackRef = s;

//    // Runtime
//    private FSM<EnemyInputs> fsm;
//    private Animator animator;
//    private Rigidbody2D rb;
//    private SpriteRenderer sr;
//    private EnemyHealth health;

//    // Exposición para estados
//    public Transform Player => player;
//    public Transform[] Waypoints => waypoints;
//    public Animator Anim => animator;
//    public Rigidbody2D Body => rb;
//    public SpriteRenderer SR => sr;

//    // FSM helpers
//    public void Transition(EnemyInputs input) => fsm?.Transition(input);


//    // Prefabs/tuning getters para AttackState
//    public GameObject StraightPrefab => straightBulletPrefab;
//    public GameObject HomingPrefab => homingBulletPrefab;
//    public GameObject ExplosivePrefab => explosiveBulletPrefab;
//    public GameObject ChildPrefab => childBulletPrefab;

//    public float StraightSpeed => straightSpeed;
//    public float HomingSpeed => homingSpeed;
//    public float HomingTurn => homingTurnDegPerSec;
//    public float ExplosiveSpeed => explosiveSpeed;
//    public float ChildSpeed => childSpeed;

//    public float StraightDamage => straightDamage;
//    public float HomingDamage => homingDamage;
//    public float ChildDamage => childDamage;

//    public LayerMask PlayerMask => playerMask;

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        if (rb)
//        {
//            rb.gravityScale = 0f;                          // forzar sin gravedad
//            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
//        }
//    }

//    private void Start()
//    {
//        EnemyManager.Instance.RegisterEnemy();

//        animator = GetComponent<Animator>();
//        rb = GetComponent<Rigidbody2D>();
//        sr = GetComponent<SpriteRenderer>();
//        health = GetComponent<EnemyHealth>();

//        if (health != null)
//        {
//            health.OnDeath += () => Transition(EnemyInputs.Die);
//            maxHealthAtSpawn = Mathf.Max(health.GetCurrentHealth(), 1f);
//        }
//        if (rb != null) rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

//        if (animator) animator.SetTrigger("Emerge");

//        StartCoroutine(_StartCycleNextFrame());

//        // Estados
//        var idle = new TopoIdleState(this);
//        var patrol = new TopoPatrolState(this);     
//        var attack = new TopoAttackState(this);     
//        var death = new EnemyDeathState(this);     

//        fsm = new FSM<EnemyInputs>(idle);

//        // Transiciones:
//        // Detección básica
//        //idle.AddTransition(EnemyInputs.SeePlayer, patrol);
//        idle.AddTransition(EnemyInputs.Die, death);

//        // Ciclo waypoint , idle , attack , idle , patrol
//        //patrol.AddTransition(EnemyInputs.SeePlayer, idle); 
//        patrol.AddTransition(EnemyInputs.Die, death);

//        idle.AddTransition(EnemyInputs.SpecialAttack, attack);
//        //attack.AddTransition(EnemyInputs.SeePlayer, idle);

//        // Reenganche a patrol luego del segundo idle
//        //idle.AddTransition(EnemyInputs.LostPlayer, patrol);

//    }

//    private void Update()
//    {
//        fsm.Update();

//        //if (isUnderground || fsm.GetCurrentState() is TopoAttackState) return;

//        // visión simple (si el topo no está “en muerte”)
//        if (!(fsm.GetCurrentState() is EnemyDeathState))
//        {
//            float dist = player ? Vector2.Distance(transform.position, player.position) : 999f;
//            if (dist <= detectionRadius) Transition(EnemyInputs.SeePlayer);
//            else Transition(EnemyInputs.LostPlayer);
//        }

//        // flip (mirar al player cuando está visible)
//        if (player && sr && sr.enabled)
//        {
//            Vector2 dir = player.position - transform.position;
//            sr.flipX = dir.x < 0;
//        }
//    }

//    // ------------------- ANIMATION EVENTS -------------------


//    public void AE_OnEmergeEnd()
//    {
//        // apenas termina el emerge, pedimos ir a Attack
//        if (animator) animator.SetTrigger("Attack");
//        Transition(EnemyInputs.SpecialAttack); // entra al TopoAttackState
//    }

//    public void AE_OnAttackFire()
//    {
//        // cada vez que el clip llama este evento, disparamos un "volley"
//        _attackRef?.FireVolleyByPhase();
//    }

//    public void AE_OnAttackEnd()
//    {
//        // al terminar el clip de ataque, arrancamos Burrow
//        if (animator) animator.SetTrigger("Burrow");
//    }

//    public void AE_OnBurrowHide() // opcional: para ocultar en un frame específico
//    {
//        if (sr) sr.enabled = false;
//        var col = GetComponent<Collider2D>();
//        if (col) col.enabled = false;
//        isUnderground = true;
//    }

//    public void AE_OnBurrowEnd()
//    {
//        // si no usaste AE_OnBurrowHide antes, ocultamos acá
//        if (sr) sr.enabled = false;
//        var col = GetComponent<Collider2D>();
//        if (col) col.enabled = false;
//        isUnderground = true;

//        // comenzamos viaje bajo tierra hacia un waypoint aleatorio
//        if (travelCR != null) StopCoroutine(travelCR);
//        travelCR = StartCoroutine(UndergroundTravelToRandomWaypoint());
//    }

//    // ------------------- VIAJE BAJO TIERRA -------------------
//    private System.Collections.IEnumerator UndergroundTravelToRandomWaypoint()
//    {
//        if (rb) rb.linearVelocity = Vector2.zero;

//        // elegimos waypoint destino distinto al actual si se puede
//        var wps = Waypoints;
//        if (wps == null || wps.Length == 0)
//        {
//            FinishUndergroundTravel(transform.position);
//            yield break;
//        }

//        // toma uno aleatorio
//        Transform target = GetRandomWaypointNotLast();
//        if (target == null)
//        {
//            FinishUndergroundTravel(transform.position);
//            yield break;
//        }

//        // Guardamos “a dónde vamos” para actualizar last al llegar
//        pendingWaypointIndex = IndexOfWaypoint(target);

//        float dist = Vector2.Distance(transform.position, target.position);
//        float speedUG = Mathf.Max(0.01f, GetUndergroundSpeedByPhase());
//        float travelTime = dist / speedUG;

//        yield return new WaitForSeconds(travelTime);

//        FinishUndergroundTravel(target.position);
//    }

//    private void FinishUndergroundTravel(Vector3 arrivePos)
//    {
//        // teletransportamos al destino y emergemos
//        transform.position = new Vector3(arrivePos.x, arrivePos.y, transform.position.z);

//        if (sr) sr.enabled = true;
//        var col = GetComponent<Collider2D>();
//        if (col) col.enabled = true;

//        isUnderground = false;
//        if (animator) animator.SetTrigger("Emerge");

//        lastWaypointIndex = pendingWaypointIndex;
//        pendingWaypointIndex = -1;
//    }

//    public float GetHealthRatio()
//    {
//        if (!health) return 1f;
//        float cur = Mathf.Max(0f, health.GetCurrentHealth());
//        return Mathf.Clamp01(cur / Mathf.Max(0.0001f, maxHealthAtSpawn));
//    }

//    public int GetPhase() // 1, 2 o 3
//    {
//        float r = GetHealthRatio();
//        if (r >= phase1Threshold) return 1;
//        if (r >= phase2Threshold) return 2;
//        return 3;
//    }

//    public float GetUndergroundSpeedByPhase()
//    {
//        switch (GetPhase())
//        {
//            case 1: return undergroundSpeedPhase1;
//            case 2: return undergroundSpeedPhase2;
//            default: return undergroundSpeedPhase3;
//        }
//    }

//    public float GetAttackDelayByPhase()
//    {
//        switch (GetPhase())
//        {
//            case 1: return attackDelayPhase1;
//            case 2: return attackDelayPhase2;
//            default: return attackDelayPhase3;
//        }
//    }

//    public int GetVolleysByPhase()
//    {
//        switch (GetPhase())
//        {
//            case 1: return volleysPhase1;
//            case 2: return volleysPhase2;
//            default: return volleysPhase3;
//        }
//    }

//    private int IndexOfWaypoint(Transform t)
//    {
//        if (waypoints == null) return -1;
//        for (int i = 0; i < waypoints.Length; i++)
//            if (waypoints[i] == t) return i;
//        return -1;
//    }

//    // Devuelve un waypoint aleatorio distinto al último usado.
//    // Si solo hay 1 válido, lo devuelve (no hay forma de evitar repetición).
//    private Transform GetRandomWaypointNotLast()
//    {
//        if (waypoints == null || waypoints.Length == 0) return null;


//        var valid = new System.Collections.Generic.List<Transform>();
//        for (int i = 0; i < waypoints.Length; i++)
//        {
//            var w = waypoints[i];
//            if (!w) continue;
//            if (i == lastWaypointIndex && waypoints.Length > 1) continue; // evitar repetir si hay más de 1
//            valid.Add(w);
//        }

//        if (valid.Count == 0) // solo había el último o todos nulos
//        {
//            // fallback: primer válido
//            for (int i = 0; i < waypoints.Length; i++)
//                if (waypoints[i]) return waypoints[i];
//            return null;
//        }

//        return valid[Random.Range(0, valid.Count)];
//    }

//    private void OnEnable()
//    {
//        if (rb) rb.gravityScale = 0f;                      // por si algún clip/prefab lo pisa
//    }

//    private System.Collections.IEnumerator _StartCycleNextFrame()
//    {
//        yield return null;               // espera 1 frame (Animator listo)
//        animator.ResetTrigger("Emerge");
//        animator.SetTrigger("Emerge");   // comienza ciclo
//    }

//    // IEnemyDataProvider
//    public Transform GetPlayer() => player;
//    public float GetDetectionRadius() => detectionRadius;
//    public float GetAttackDistance() => detectionRadius;
//    public float GetDamage() => 0f; // no melee
//    public float GetMaxSpeed() => baseMaxSpeed;
//    public float GetAcceleration() => baseAcceleration;
//    public float GetCurrentHealth() => health ? health.GetCurrentHealth() : 0f;
//}
