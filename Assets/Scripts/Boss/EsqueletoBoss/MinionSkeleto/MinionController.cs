using UnityEngine;
using System.Collections;

public class MinionController : MonoBehaviour, IEnemyDataProvider, IMeleeHost
{
    [Header("References")][SerializeField] private Transform player;
    [Header("Stats")][SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float maxSpeed = 2.5f;

    [Header("Spawn")]
    [Tooltip("No se usa con Animation Event")]
    [SerializeField] private float spawnAnimDuration = 1f;

    [SerializeField] private float meleeAttackRange = 1.0f;
    [SerializeField] private float meleeCooldown = 0.7f;
    private float nextMeleeAllowedTime = 0f;

    [SerializeField] private EnemyAttack attack;

    private FSM<EnemyInputs> fsm;
    private Animator animator;
    private SpriteRenderer sprite;
    private EnemyHealth health;
    private Rigidbody2D rb;

    // === Hook para Animation Event ===
    private MinionSpawnState _spawnStateRef;
    public void RegisterSpawnState(MinionSpawnState s) => _spawnStateRef = s;
    public void OnMinionSpawnFinished() => _spawnStateRef?.NotifySpawnFinished();

    public bool IsSpawning() => fsm.GetCurrentState() is MinionSpawnState;

    private MeleeAttackState _meleeRef;
    public void RegisterMeleeState(MeleeAttackState s) => _meleeRef = s;

    // events desde Animator (clip Melee del minion)
    public void OnMeleeHit() => _meleeRef?.OnMeleeHit();
    public void OnMeleeFinished() => _meleeRef?.OnMeleeFinished();

    // helpers para el árbol
    public bool IsMeleeing() => fsm.GetCurrentState() is MeleeAttackState;
    public bool CanMeleeAttack() => Time.time >= nextMeleeAllowedTime;
    public bool IsPlayerInMeleeRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= meleeAttackRange;
    }
    public void MarkMeleeUsed() => nextMeleeAllowedTime = Time.time + meleeCooldown;

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();

        if (health != null) health.OnDeath += () => Transition(EnemyInputs.Die);

        if (rb != null) rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        var spawn = new MinionSpawnState(this);      // sin duration
        var idle = new EnemyIdleState(transform);
        var follow = new EnemyFollowState(transform, player, maxSpeed);
        var death = new EnemyDeathState(this);
        var melee = new MeleeAttackState(this);

        fsm = new FSM<EnemyInputs>(spawn);

        spawn.AddTransition(EnemyInputs.SeePlayer, follow);
        spawn.AddTransition(EnemyInputs.Die, death);

        idle.AddTransition(EnemyInputs.SeePlayer, follow);
        idle.AddTransition(EnemyInputs.Die, death);

        follow.AddTransition(EnemyInputs.LostPlayer, idle);
        follow.AddTransition(EnemyInputs.Die, death);

        follow.AddTransition(EnemyInputs.MeleeAttack, melee);
        melee.AddTransition(EnemyInputs.SeePlayer, follow);
    }

    private void FixedUpdate()
    {
        // Tick fijo del FSM (para que EnemyFollowState.FixedExecute corra a paso fijo)
        fsm.FixedUpdate();
    }

    private void Update()
    {
        fsm.Update();

        if (IsSpawning() || IsMeleeing())
            return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= detectionRadius) Transition(EnemyInputs.SeePlayer);
        else Transition(EnemyInputs.LostPlayer);

        bool walking = fsm.GetCurrentState() is EnemyFollowState;
        animator.SetBool("isWalking", walking);

        if (player != null)
            sprite.flipX = (player.position.x - transform.position.x) < 0f;
    }

    public void Transition(EnemyInputs input) => fsm.Transition(input);

    // IEnemyDataProvider
    public float GetCurrentHealth() => health != null ? health.GetCurrentHealth() : 0f;
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => detectionRadius;
    public float GetDamage() => 0f;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => 0f;

    public IEnumerator DelayedDeath()
    {
        EnemyManager.Instance.UnregisterEnemy();
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    // IMeleeHost
    public Transform Transform => transform;
    public Animator Animator => animator;
    public Rigidbody2D Body => rb;
    public EnemyAttack Attack => attack;
}
//public class MinionController : MonoBehaviour, IEnemyDataProvider
//{
//    [Header("References")]
//    [SerializeField] private Transform player;

//    [Header("Stats")]
//    [SerializeField] private float detectionRadius = 4f;
//    [SerializeField] private float maxSpeed = 2.5f;

//    [Header("Spawn")]
//    [Tooltip("Duración en segundos de la anim de aparición")]
//    [SerializeField] private float spawnAnimDuration = 1f;

//    private FSM<EnemyInputs> fsm;
//    private Animator animator;
//    private SpriteRenderer spriteRenderer;
//    private EnemyHealth health;
//    private SpriteRenderer sprite;
//    private Rigidbody2D rb;

//    // Helper para “¿estoy en Spawn?”    
//    public bool IsSpawning() => fsm.GetCurrentState() is MinionSpawnState;
//    private void Start()
//    {

//        EnemyManager.Instance.RegisterEnemy();

//        if (player == null)
//        {
//            var playerObj = GameObject.FindGameObjectWithTag("Player");
//            if (playerObj != null)
//                player = playerObj.transform;
//            else
//                Debug.LogError("[MinionController] No se encontró ningún GameObject con tag 'Player'.");
//        }

//        animator = GetComponent<Animator>();
//        sprite = GetComponent<SpriteRenderer>();
//        rb = GetComponent<Rigidbody2D>();
//        health = GetComponent<EnemyHealth>();

//        if (health != null)
//            health.OnDeath += () => Transition(EnemyInputs.Die);

//        // Asegúrate de no rotar nunca
//        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

//        // FSM
//        var spawn = new MinionSpawnState(this, spawnAnimDuration);
//        var idle = new EnemyIdleState(transform);
//        var follow = new EnemyFollowState(transform, player, maxSpeed);
//        var death = new EnemyDeathState(this);

//        fsm = new FSM<EnemyInputs>(spawn);

//        spawn.AddTransition(EnemyInputs.SeePlayer, follow);
//        spawn.AddTransition(EnemyInputs.Die, death);

//        idle.AddTransition(EnemyInputs.SeePlayer, follow);
//        idle.AddTransition(EnemyInputs.Die, death);

//        follow.AddTransition(EnemyInputs.LostPlayer, idle);
//        follow.AddTransition(EnemyInputs.Die, death);
//    }

//    private void Update()
//    {
//        fsm.Update();

//        // Durante spawn, no decido nada más
//        if (IsSpawning())
//            return;

//        // Transiciones automáticas See/Lost
//        float dist = Vector2.Distance(transform.position, player.position);
//        if (dist <= detectionRadius) Transition(EnemyInputs.SeePlayer);
//        else Transition(EnemyInputs.LostPlayer);

//        // Animación de caminar
//        bool walking = fsm.GetCurrentState() is EnemyFollowState;
//        animator.SetBool("isWalking", walking);

//        // Flip X
//        if (player != null)
//            sprite.flipX = (player.position.x - transform.position.x) < 0;
//    }

//    public void Transition(EnemyInputs input) => fsm.Transition(input);


//    public IEnumerator DelayedDeath()
//    {
//        EnemyManager.Instance.UnregisterEnemy();
//        yield return new WaitForSeconds(0.5f);
//        Destroy(gameObject);
//    }
//    public float GetCurrentHealth() => health != null ? health.GetCurrentHealth() : 0f;
//    public Transform GetPlayer() => player;
//    public float GetDetectionRadius() => detectionRadius;
//    public float GetAttackDistance() => detectionRadius;
//    public float GetDamage() => 0f;
//    public float GetMaxSpeed() => maxSpeed;
//    public float GetAcceleration() => 0f;
//}
