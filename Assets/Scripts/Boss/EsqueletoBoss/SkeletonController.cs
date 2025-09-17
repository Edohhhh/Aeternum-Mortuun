using System.Collections;
using UnityEngine;

public class SkeletonController : MonoBehaviour, IEnemyDataProvider, IMeleeHost
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Minions")]
    [Tooltip("Prefab del mini-esqueleto que invoca el jefe")]
    [SerializeField] private GameObject minionPrefab;
    [Tooltip("Puntos de spawn (Transforms) donde aparecer�n los mini-esqueletos")]
    [SerializeField] private Transform[] minionSpawnPoints;

    [Header("Stats")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float acceleration = 10f;

    [Header("Spawn")]
    [Tooltip("Duraci�n en segundos de tu anim de aparici�n (no se usa con Animation Event, pero lo dejamos por si lo necesit�s para otra cosa)")]
    [SerializeField] private float spawnAnimDuration = 1f;

    [Header("Special Attacks")]
    [SerializeField] private float underGroundCooldown = 10f;
    [SerializeField] private float spawnMinionsCooldown = 15f;

    [Header("Timing")]
    [Tooltip("Segundos tras el spawn inicial antes de invocar minions")]
    [SerializeField] public float initialSpawnMinionDelay = 1.0f;

    [Tooltip("Segundos tras invocar minions antes de enterrarse")]
    [SerializeField] public float postSpawnUnderGroundDelay = 2.0f;

    [Header("Special Attacks Timing")]
    [Tooltip("Duraci�n de enterrarse (s)")]
    [SerializeField] private float undergroundBuryDuration = 0.5f;
    [Tooltip("Duraci�n de emerger (s)")]
    [SerializeField] private float undergroundEmergeDuration = 0.5f;

    [SerializeField] private float meleeAttackRange = 1.2f;
    [SerializeField] private float meleeCooldown = 0.7f;
    private float nextMeleeAllowedTime = 0f;

    [SerializeField] private EnemyAttack attack;

    public bool CanMeleeAttack() => Time.time >= nextMeleeAllowedTime;
    public bool IsPlayerInMeleeRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= meleeAttackRange;
    }
    public void MarkMeleeUsed() => nextMeleeAllowedTime = Time.time + meleeCooldown;
    private MeleeAttackState _meleeRef;
    public void RegisterMeleeState(MeleeAttackState s) => _meleeRef = s;

    // Reenv�o de Animation Events del clip de golpe
    public void OnMeleeHit() => _meleeRef?.OnMeleeHit();
    public void OnMeleeFinished() => _meleeRef?.OnMeleeFinished();

    private float lastUnderGroundTime;
    private float lastSpawnMinionsTime;
    private UnderGroundAttackState _ugStateRef;

    public void RegisterUnderGroundState(UnderGroundAttackState s) => _ugStateRef = s;
    // Llamados por Animation Events desde los clips de Animator:
    public void OnBurrowFinished() { _ugStateRef?.OnBurrowAnimFinished(); }
    public bool IsUnderGrounding() => fsm.GetCurrentState() is UnderGroundAttackState;

    public bool IsMeleeing() => fsm.GetCurrentState() is MeleeAttackState;

    private float nextUGReadyTime;
    private float nextSpawnReadyTime;

    private FSM<EnemyInputs> fsm;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth health;
    private EsqueletoSpawnState spawnState;
    private SpawnMinionState spawnMinionState;
    private Rigidbody2D rb;


    private EsqueletoSpawnState _spawnStateRef;

    public void RegisterSpawnState(EsqueletoSpawnState s) => _spawnStateRef = s;

    public void OnSpawnFinished()
    {

        _spawnStateRef?.NotifySpawnFinished();
    }

    public bool IsSpawning() => fsm.GetCurrentState() is EsqueletoSpawnState;

    public bool IsSpawningMinions() =>
        fsm.GetCurrentState() is SpawnMinionState;

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
        if (health != null) health.OnDeath += () => Transition(EnemyInputs.Die);

        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        nextUGReadyTime = Time.time + underGroundCooldown;
        nextSpawnReadyTime = float.PositiveInfinity;

        var spawn = new EsqueletoSpawnState(this, spawnAnimDuration);
        var follow = new EnemyFollowState(transform, player, maxSpeed);
        var underGround = new UnderGroundAttackState(this, undergroundBuryDuration, undergroundEmergeDuration);
        var spawnMin = new SpawnMinionState(this, minionPrefab, minionSpawnPoints, 1.5f);
        var death = new EnemyDeathState(this);
        var melee = new MeleeAttackState(this /* IMeleeHost */);

        // FSM arranca en SpawnState
        fsm = new FSM<EnemyInputs>(spawn);

        // transiciones Spawn  Follow/Death
        spawn.AddTransition(EnemyInputs.SeePlayer, follow);
        spawn.AddTransition(EnemyInputs.Die, death);

        // transiciones Follow  Specials / Death
        follow.AddTransition(EnemyInputs.SpawnMinions, spawnMin);
        follow.AddTransition(EnemyInputs.Die, death);

        // mapea la transici�n desde Follow:
        follow.AddTransition(EnemyInputs.UnderGroundAttack, underGround);
        underGround.AddTransition(EnemyInputs.SeePlayer, follow);

        // transiciones SpawnMinions  Follow/Death
        spawnMin.AddTransition(EnemyInputs.SeePlayer, follow);
        spawnMin.AddTransition(EnemyInputs.Die, death);

        follow.AddTransition(EnemyInputs.MeleeAttack, melee);
        melee.AddTransition(EnemyInputs.SeePlayer, follow);

        underGround.AddTransition(EnemyInputs.Spawn, spawn);

        lastUnderGroundTime = -underGroundCooldown;
        lastSpawnMinionsTime = -spawnMinionsCooldown;
    }

    private void FixedUpdate()
    {
        // Tick fijo del FSM (para que EnemyFollowState.FixedExecute corra a paso fijo)
        fsm.FixedUpdate();
    }

    private void Update()
    {
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        // 1) Actualizamos la FSM
        fsm.Update();

        // 2) Si seguimos en spawn/minions/underground, no procesamos visi�n ni movimiento
        if (IsSpawning() || IsSpawningMinions() || fsm.GetCurrentState() is UnderGroundAttackState)
            return;


        // 3) Detecci�n normal
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= detectionRadius)
            Transition(EnemyInputs.SeePlayer);
        else
            Transition(EnemyInputs.LostPlayer);

        bool walking = fsm.GetCurrentState() is EnemyFollowState;
        animator.SetBool("isWalking", walking);



        // 4) Flip del sprite
        if (player != null)
        {
            Vector2 dir = player.position - transform.position;
            spriteRenderer.flipX = dir.x < 0;
        }
    }

    public bool CanUseUnderGroundAttack() => Time.time >= nextUGReadyTime;
    public bool CanUseSpawnMinions() => Time.time >= nextSpawnReadyTime;

    public void MarkUnderGroundUsed()
    {
        // se us�: pr�ximo permiso luego del cooldown
        nextUGReadyTime = Time.time + underGroundCooldown;
    }

    public void MarkSpawnMinionsUsed()
    {
        nextSpawnReadyTime = Time.time + spawnMinionsCooldown;
        // tras invocar minions, programa una ventana para permitir UG
        nextUGReadyTime = Mathf.Max(nextUGReadyTime, Time.time + postSpawnUnderGroundDelay);
    }



    public void DoUnderGroundAttack()
    {
        if (fsm.GetCurrentState() is UnderGroundAttackState || fsm.GetCurrentState() is SpawnMinionState)
            return;

        MarkUnderGroundUsed();
        Transition(EnemyInputs.UnderGroundAttack);
    }

    public void DoSpawnMinions()
    {
        if (fsm.GetCurrentState() is SpawnMinionState || fsm.GetCurrentState() is UnderGroundAttackState)
            return;


        if (float.IsPositiveInfinity(nextSpawnReadyTime))
            nextSpawnReadyTime = Time.time; // habilita primera llamada

        MarkSpawnMinionsUsed();
        Transition(EnemyInputs.SpawnMinions);
    }

    public void Transition(EnemyInputs input) => fsm.Transition(input);

    // IEnemyDataProvider (resto)
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => detectionRadius;
    public float GetDamage() => 0f;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;
    public float GetCurrentHealth() => health != null ? health.GetCurrentHealth() : 0f;

    // IMeleeHost (resto)
    public Transform Transform => transform;
    public Animator Animator => animator;
    public Rigidbody2D Body => rb;
    public EnemyAttack Attack => attack;
}
//public class SkeletonController : MonoBehaviour, IEnemyDataProvider
//{
//    [Header("References")]
//    [SerializeField] private Transform player;

//    [Header("Minions")]
//    [Tooltip("Prefab del mini-esqueleto que invoca el jefe")]
//    [SerializeField] private GameObject minionPrefab;
//    [Tooltip("Puntos de spawn (Transforms) donde aparecer�n los mini-esqueletos")]
//    [SerializeField] private Transform[] minionSpawnPoints;

//    [Header("Stats")]
//    [SerializeField] private float detectionRadius = 5f;
//    [SerializeField] private float maxSpeed = 3f;
//    [SerializeField] private float acceleration = 10f;

//    [Header("Spawn")]
//    [Tooltip("Duraci�n en segundos de tu anim de aparici�n")]
//    [SerializeField] private float spawnAnimDuration = 1f;

//    [Header("Special Attacks")]
//    [SerializeField] private float underGroundCooldown = 10f;
//    [SerializeField] private float spawnMinionsCooldown = 15f;

//    [Header("Timing")]
//    [Tooltip("Segundos tras el spawn inicial antes de invocar minions")]
//    [SerializeField] public float initialSpawnMinionDelay = 1.0f;

//    [Tooltip("Segundos tras invocar minions antes de enterrarse")]
//    [SerializeField] public float postSpawnUnderGroundDelay = 2.0f;

//    [Header("Special Attacks Timing")]
//    [Tooltip("Duraci�n de enterrarse (s)")]
//    [SerializeField] private float undergroundBuryDuration = 0.5f;
//    [Tooltip("Duraci�n de emerger (s)")]
//    [SerializeField] private float undergroundEmergeDuration = 0.5f;


//    private float lastUnderGroundTime;
//    private float lastSpawnMinionsTime;


//    private FSM<EnemyInputs> fsm;
//    private Animator animator;
//    private SpriteRenderer spriteRenderer;
//    private EnemyHealth health;
//    private EsqueletoSpawnState spawnState;
//    private SpawnMinionState spawnMinionState;
//    private Rigidbody2D rb;

//    private EsqueletoSpawnState _spawnStateRef;



//    public void RegisterSpawnState(EsqueletoSpawnState s) => _spawnStateRef = s;

//    // Helper para bloquear visi�n/movimiento durante el spawn
//    public bool IsSpawning() => fsm.GetCurrentState() is EsqueletoSpawnState;

//    public bool IsSpawningMinions() =>
//    fsm.GetCurrentState() is SpawnMinionState;

//    private void Start()
//    {
//        EnemyManager.Instance.RegisterEnemy();

//        animator = GetComponent<Animator>();
//        spriteRenderer = GetComponent<SpriteRenderer>();
//        health = GetComponent<EnemyHealth>();
//        rb = GetComponent<Rigidbody2D>();
//        if (health != null) health.OnDeath += () => Transition(EnemyInputs.Die);

//        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

//        var spawn = new EsqueletoSpawnState(this, spawnAnimDuration);
//        var follow = new EnemyFollowState(transform, player, maxSpeed);
//        var underGround = new UnderGroundAttackState(
//        this,
//        undergroundBuryDuration,
//        undergroundEmergeDuration
//    );
//        var spawnMin = new SpawnMinionState(
//     this,
//     minionPrefab,
//     minionSpawnPoints,
//     /* aqu� la duraci�n real de tu anim: */ 1.5f
// );
//        //var spawnMin = spawnMinionState;
//        var death = new EnemyDeathState(this);

//        // FSM arranca en SpawnState
//        fsm = new FSM<EnemyInputs>(spawn);

//        // transiciones Spawn  Follow/Death
//        spawn.AddTransition(EnemyInputs.SeePlayer, follow);
//        spawn.AddTransition(EnemyInputs.Die, death);

//        // transiciones Follow  Specials / Death
//        //follow.AddTransition(EnemyInputs.UnderGroundAttack, underGround);
//        follow.AddTransition(EnemyInputs.SpawnMinions, spawnMin);
//        follow.AddTransition(EnemyInputs.Die, death);

//        // mapea la transici�n desde Follow:
//        follow.AddTransition(EnemyInputs.UnderGroundAttack, underGround);
//        underGround.AddTransition(EnemyInputs.SeePlayer, follow);

//        // transiciones SpawnMinions  Follow/Death
//        spawnMin.AddTransition(EnemyInputs.SeePlayer, follow);
//        spawnMin.AddTransition(EnemyInputs.Die, death);

//        lastUnderGroundTime = -underGroundCooldown;
//        lastSpawnMinionsTime = -spawnMinionsCooldown;
//    }

//    private void Update()
//    {
//        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

//        // 1) Actualizamos la FSM
//        fsm.Update();

//        // 2) Si seguimos en spawn, no procesamos visi�n ni movimiento
//        if (IsSpawning() || IsSpawningMinions() || fsm.GetCurrentState() is UnderGroundAttackState)
//            return;

//        // 3) Una vez sali� del spawn, procesamos detecci�n normal
//        float dist = Vector2.Distance(transform.position, player.position);
//        if (dist <= detectionRadius)
//            Transition(EnemyInputs.SeePlayer);
//        else
//            Transition(EnemyInputs.LostPlayer);

//        bool walking = fsm.GetCurrentState() is EnemyFollowState;
//        animator.SetBool("isWalking", walking);

//        // 4) Flip del sprite
//        if (player != null)
//        {
//            Vector2 dir = player.position - transform.position;
//            spriteRenderer.flipX = dir.x < 0;
//        }
//    }

//    public bool CanUseUnderGroundAttack() =>
//       Time.time >= lastUnderGroundTime + underGroundCooldown;

//    public bool CanUseSpawnMinions() =>
//        Time.time >= lastSpawnMinionsTime + spawnMinionsCooldown;

//    public void MarkUnderGroundUsed() => lastUnderGroundTime = Time.time;
//    public void MarkSpawnMinionsUsed() => lastSpawnMinionsTime = Time.time;

//    public void DoUnderGroundAttack()
//    {
//        // Si ya estamos enterrados o invocando, no hacer nada
//        if (fsm.GetCurrentState() is UnderGroundAttackState ||
//            fsm.GetCurrentState() is SpawnMinionState)
//            return;

//        MarkUnderGroundUsed();
//        Transition(EnemyInputs.UnderGroundAttack);
//    }

//    public void DoSpawnMinions()
//    {
//        // Si ya estamos invocando o enterrados, no hacer nada
//        if (fsm.GetCurrentState() is SpawnMinionState ||
//            fsm.GetCurrentState() is UnderGroundAttackState)
//            return;

//        MarkSpawnMinionsUsed();
//        Transition(EnemyInputs.SpawnMinions);
//    }

//    //public void OnSpawnMinionsAnimationComplete()
//    //{
//    //    // Reenv�a la se�al al estado para que pase a instanciar
//    //    spawnMinionState.FinishSpawnMinions();
//    //}


//    public void Transition(EnemyInputs input) => fsm.Transition(input);

//    // IEnemyDataProvider (resto)
//    public Transform GetPlayer() => player;
//    public float GetDetectionRadius() => detectionRadius;
//    public float GetAttackDistance() => detectionRadius;
//    public float GetDamage() => 0f;
//    public float GetMaxSpeed() => maxSpeed;
//    public float GetAcceleration() => acceleration;
//    public float GetCurrentHealth() => health != null ? health.GetCurrentHealth() : 0f;
//}

