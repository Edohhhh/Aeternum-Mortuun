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
    [SerializeField] public float laserRecoverTime = 0f;

    [Header("Laser Prefab")]
    [SerializeField] public GolemBeam laserPrefab;          // arrastrá el prefab acá desde el inspector
    public GolemBeam LaserPrefab => laserPrefab;

    [Header("Laser Tuning")]
    [SerializeField] private float beamDuration = 3f;      // duración visual del rayo
    [SerializeField] private float beamDamage = 1f;      // daño por tick/impacto
    [SerializeField] private float beamMaxRange = 12f;     // alcance máx.
    [SerializeField] private float beamThickness = 0.6f;    // grosor visual/colisión
    [SerializeField] private float beamKnockback = 6f;      // 0 para desactivar empuje
    [SerializeField] private LayerMask beamPlayerMask;       // capa Player
    [SerializeField] private LayerMask beamObstacleMask;     // muros

    //[Header("Heavy Attack")]
    //[SerializeField] private float heavyCooldown = 15f;
    //[SerializeField] private float heavyAirTime = 0.6f;     // tiempo fijo en el aire
    //[SerializeField] private float heavyArcHeight = 0.6f;   // altura del arco visual
    //[SerializeField] private float heavyStunTime = 4f;      // se queda stuneado
    //[SerializeField] private float heavyDamage = 2f;
    //[SerializeField] private float heavyDamageRadius = 1.5f;

    //private float heavyElapsed;

    // Expuestos al estado
    //public float HeavyAirTime => heavyAirTime;
    //public float HeavyArcHeight => heavyArcHeight;
    //public float HeavyStunTime => heavyStunTime;
    //public float HeavyDamage => heavyDamage;
    //public float HeavyDamageRadius => heavyDamageRadius;

    //private GolemHeavyAttackState _heavyRef;
    //public void RegisterHeavyState(GolemHeavyAttackState s) => _heavyRef = s;
    //public bool IsHeavyAttacking() => fsm.GetCurrentState() is GolemHeavyAttackState;

    // Animator Events del clip Heavy
    //public void OnHeavyJumpStart() => _heavyRef?.OnJumpStart();
    //public void OnHeavyImpact() => _heavyRef?.OnImpact();
    //public void OnHeavyFinished() => _heavyRef?.OnFinished();

    // Gating del heavy (con timer interno)
    //public bool CanUseHeavy() => heavyElapsed >= heavyCooldown;
    //public void MarkHeavyUsed() => heavyElapsed = 0f;


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
        //var heavy = new GolemHeavyAttackState(this);    
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

        //follow.AddTransition(EnemyInputs.HeavyAttack, heavy);
        //heavy.AddTransition(EnemyInputs.SeePlayer, follow);
        //heavy.AddTransition(EnemyInputs.Die, death);
    }

    private void Update()
    {
        // Mientras golpea, no meter inputs de ver/seguir
        if (IsMeleeing() || IsLasering() /*|| IsHeavyAttacking()*/)
        {
            fsm.Update();
            if (!IsLasering()) /*heavyElapsed += Time.deltaTime;*/
            fsm.Update();
            return;
        }

        fsm.Update();
        //heavyElapsed += Time.deltaTime;

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

    // Laser 
    public float BeamDuration => beamDuration;
    public float BeamDmg => beamDamage;
    public float BeamMaxRange => beamMaxRange;
    public float BeamThickness => beamThickness;
    public float BeamKnockback => beamKnockback;
    public LayerMask BeamPlayerMask => beamPlayerMask;
    public LayerMask BeamObstacleMask => beamObstacleMask;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }
}
