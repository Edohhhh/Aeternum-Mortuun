using UnityEngine;

public class GolemController : MonoBehaviour, IEnemyDataProvider, IMeleeHost
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Stats")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float maxSpeed = 2.2f;
    [SerializeField] private float acceleration = 10f;

    [Header("Melee")]
    [SerializeField] private float meleeAttackRange = 1.4f;
    [SerializeField] private float meleeCooldown = 0.9f;
    private float nextMeleeAllowedTime = 0f;

    [Header("Attack script (del prefab)")]
    [SerializeField] private EnemyAttack attack; // asignalé el EnemyAttack del golem

    [Header("Laser Special")]
    [SerializeField] private float laserInitialDelay = 15f;   
    [SerializeField] private float laserCooldown = 12f;
    [SerializeField] public float laserRecoverTime = 2f;
    

    private float nextLaserReadyTime;

    // gating
    public bool CanUseLaser() => Time.time >= nextLaserReadyTime;
    public void MarkLaserUsed() => nextLaserReadyTime = Time.time + laserCooldown;
    public bool IsPlayerOutsideMelee() => !IsPlayerInMeleeRange();

    // referencia al estado (para reenviar Animation Events)
    private GolemLaserState _laserRef;
    public void RegisterLaserState(GolemLaserState s) => _laserRef = s;
    public bool IsLasering() => fsm.GetCurrentState() is GolemLaserState;

    // Eventos llamados por la animación (los pondrás en el clip)
    public void OnLaserChargeEnd() => _laserRef?.OnChargeEnd();     
    public void OnLaserFinished() => _laserRef?.OnLaserFinished(); 

    // Ref del estado Melee para reenviar eventos de animación
    private MeleeAttackState _meleeRef;
    public void RegisterMeleeState(MeleeAttackState s) => _meleeRef = s;

    // Animator Events (clip Melee)
    public void OnMeleeHit() => _meleeRef?.OnMeleeHit();
    public void OnMeleeFinished()
    {
        Debug.Log("CTRL (Golem) OnMeleeFinished");
        _meleeRef?.OnMeleeFinished();
    }

    // Helpers de estado
    public bool IsMeleeing() => fsm.GetCurrentState() is MeleeAttackState;
    public bool CanMeleeAttack() => Time.time >= nextMeleeAllowedTime;
    public void MarkMeleeUsed() => nextMeleeAllowedTime = Time.time + meleeCooldown;
    public bool IsPlayerInMeleeRange()
    {
        if (!player) return false;
        return Vector2.Distance(transform.position, player.position) <= meleeAttackRange;
    }

    public bool IsFollowing() => fsm.GetCurrentState() is EnemyFollowState;

    // FSM & comps
    private FSM<EnemyInputs> fsm;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth health;
    private Rigidbody2D rb;

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();

        if (health != null) health.OnDeath += () => Transition(EnemyInputs.Die);
        if (rb != null) rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        nextLaserReadyTime = Time.time + laserInitialDelay;

        // Estados base
        var idle = new EnemyIdleState(transform);
        var follow = new EnemyFollowState(transform, player, maxSpeed);
        var melee = new MeleeAttackState(this);
        var laser = new GolemLaserState(this);
        var death = new EnemyDeathState(this);

        fsm = new FSM<EnemyInputs>(idle);

        // Transiciones
        idle.AddTransition(EnemyInputs.SeePlayer, follow);
        idle.AddTransition(EnemyInputs.Die, death);

        follow.AddTransition(EnemyInputs.LostPlayer, idle);
        follow.AddTransition(EnemyInputs.MeleeAttack, melee);
        follow.AddTransition(EnemyInputs.Die, death);

        melee.AddTransition(EnemyInputs.SeePlayer, follow);

        follow.AddTransition(EnemyInputs.SpecialAttack, laser);
        laser.AddTransition(EnemyInputs.SeePlayer, follow);
        laser.AddTransition(EnemyInputs.Die, death);
    }

    private void Update()
    {
        // Mientras golpea, no meter inputs de ver/seguir
        if (IsMeleeing() || IsLasering())
        {
            fsm.Update();
            return;
        }

        fsm.Update();

        // Ver / perder jugador
        float dist = player ? Vector2.Distance(transform.position, player.position) : 999f;
        if (dist <= detectionRadius) Transition(EnemyInputs.SeePlayer);
        else Transition(EnemyInputs.LostPlayer);

        // Anim caminar
        animator.SetBool("isWalking", fsm.GetCurrentState() is EnemyFollowState);

        // Flip
        if (player)
        {
            Vector2 dir = player.position - transform.position;
            spriteRenderer.flipX = dir.x < 0;
        }
    }

    public void Transition(EnemyInputs input) => fsm.Transition(input);

    // IEnemyDataProvider
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => detectionRadius;
    public float GetDamage() => 0f;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;
    public float GetCurrentHealth() => health ? health.GetCurrentHealth() : 0f;

    // IMeleeHost (para MeleeAttackState)
    public Transform Transform => transform;
    public Animator Animator => animator;
    public Rigidbody2D Body => rb;
    public EnemyAttack Attack => attack;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }
}
