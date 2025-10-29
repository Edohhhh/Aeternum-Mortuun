using UnityEngine;

public class MimicoController : MonoBehaviour, IEnemyDataProvider, IMeleeHost
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Stats")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float maxSpeed = 2.4f;
    [SerializeField] private float acceleration = 10f;

    [Header("Melee")]
    [SerializeField] private float meleeAttackRange = 1.35f;
    [SerializeField] private float meleeCooldown = 0.9f;
    private float nextMeleeAllowedTime = 0f;

    [Header("Interacción")]
    [SerializeField] private float interactRadius = 1.1f;
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    [Header("Dormant Icon (F)")]
    [SerializeField] private Sprite interactIcon;
    [SerializeField] private float spriteOffsetY = 1.6f;
    [SerializeField] private string iconSortingLayer = "UI";
    [SerializeField] private int iconSortingOrder = 50;

    [Header("Dormant/Awaken")]
    [SerializeField] private bool toggleAllColliders = true;
    [SerializeField] private Collider2D[] collidersToToggle;   // si toggleAllColliders = false, asigná acá

    [Header("Special")]
    [SerializeField] private Transform rightSpawn;
    [SerializeField] private Transform leftEdge;
    [SerializeField] private Transform[] lanesBlue; // size = 3
    [SerializeField] private Transform[] lanesRed;  // size = 2

    [SerializeField] private float prepTime = 1f;
    [SerializeField] private float recoverTime = 2f;

    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float emitterSpeed = 8f;

    [SerializeField] private GameObject spikeTrapPrefab;
    [SerializeField] private float trapDuration = 8f;
    [SerializeField] private float trailStep = 0.6f;

    [SerializeField] private float specialCooldown = 12f;
    private float specialCharge = 0f;
    private float nextSpecialReadyTime;
    [SerializeField] private ContactDamager chargeDamager;
    public void SetChargeDamage(bool enabled)
    {
        if (chargeDamager) chargeDamager.SetEnabled(enabled);
    }
    public Transform RightSpawn => rightSpawn;
    public Transform LeftEdge => leftEdge;
    public Transform[] LanesBlue => lanesBlue;
    public Transform[] LanesRed => lanesRed;

    public float PrepTime => prepTime;
    public float RecoverTime => recoverTime;
    public float ChargeSpeed => chargeSpeed;
    public float EmitterSpeed => emitterSpeed;
    public GameObject SpikeTrapPrefab => spikeTrapPrefab;
    public float TrapDuration => trapDuration;
    public float TrailStep => trailStep;

    public bool CanUseSpecial() => specialCharge >= specialCooldown;
    public void MarkSpecialUsed() => specialCharge = 0f;
    public bool IsSpecialing() => fsm.GetCurrentState() is MimicoSpecialState;

    [SerializeField] public GameObject colliderDummy;

    public void SetDummyColliderActive(bool v)
    {
        if (colliderDummy != null)
            colliderDummy.SetActive(v);
    }


    [Header("Attack script (del prefab)")]
    [SerializeField] private EnemyAttack attack; // asignalé el EnemyAttack del Mimico

    // FSM & comps
    private FSM<EnemyInputs> fsm;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth health;
    private Rigidbody2D rb;

    // Refs de estados (para reenviar Animation Events)
    private MeleeAttackState _meleeRef;
    private EnemyDeathState _deathRef;
    private MimicoAwakenState _awakenRef;

    public float InteractRadius => interactRadius;
    public KeyCode InteractKey => interactKey;

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();

        if (health != null) health.OnDeath += () => Transition(EnemyInputs.Die);
        if (rb != null) rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        // Estados base
        var dormant = new MimicoDormantState(this);   // 1º estado
        var awaken = new MimicoAwakenState(this);
        var idle = new EnemyIdleState(transform);
        var follow = new EnemyFollowState(transform, player, maxSpeed);
        var melee = new MeleeAttackState(this);
        var special = new MimicoSpecialState(this);
        var death = new EnemyDeathState(this);

        fsm = new FSM<EnemyInputs>(dormant);

        // === TRANSICIONES ===
        // Dormant -> Awaken (por F cerca)
        dormant.AddTransition(EnemyInputs.SpecialAttack, awaken);
        dormant.AddTransition(EnemyInputs.Die, death);

        // Awaken -> Follow (desde Animation Event del clip)
        awaken.AddTransition(EnemyInputs.SeePlayer, follow);
        awaken.AddTransition(EnemyInputs.Die, death);

        // Transiciones (mismo estilo que Golem)
        idle.AddTransition(EnemyInputs.SeePlayer, follow);
        idle.AddTransition(EnemyInputs.Die, death);

        follow.AddTransition(EnemyInputs.LostPlayer, idle);
        follow.AddTransition(EnemyInputs.MeleeAttack, melee);
        follow.AddTransition(EnemyInputs.Die, death);

        // Melee vuelve a Follow por SeePlayer (idéntico patrón)
        melee.AddTransition(EnemyInputs.SeePlayer, follow);

        // Transiciones (SpecialAtack)
        follow.AddTransition(EnemyInputs.SpecialAttack, special);
        special.AddTransition(EnemyInputs.SeePlayer, follow);
        special.AddTransition(EnemyInputs.Die, death);

        // Registrar refs de estados (para Animation Events)
        RegisterMeleeState(melee);
        RegisterDeathState(death);

        if (animator != null)
        {
            animator.Play("Dormant", 0, 0f);
        }

        SetChargeDamage(false);
    }

    private void FixedUpdate()
    {
        // Tick fijo del FSM (para Follow con físicas)
        fsm.FixedUpdate();
    }

    private void Update()
    {
        // Mientras golpea, no metemos inputs externos
        if (IsDormant() || IsAwakening() || IsMeleeing() || IsSpecialing())
        {
            fsm.Update();
            return;
        }

        fsm.Update();

        // Solo procesar visión y caminar si NO está atacando
        float dist = player ? Vector2.Distance(transform.position, player.position) : 999f;
        if (dist <= detectionRadius)
            Transition(EnemyInputs.SeePlayer);
        else
            Transition(EnemyInputs.LostPlayer);

        // Anim caminar
        if (animator) animator.SetBool("isWalking", fsm.GetCurrentState() is EnemyFollowState);

        bool blocked = IsDormant() || IsAwakening() || IsMeleeing() || IsSpecialing();
        if (!blocked && IsFollowing())
            specialCharge += Time.deltaTime;

        // Flip
        if (player && spriteRenderer)
        {
            Vector2 dir = player.position - transform.position;
            spriteRenderer.flipX = dir.x < 0;
        }
    }

    public void Transition(EnemyInputs input) => fsm.Transition(input);

    // ===== IEnemyDataProvider =====
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => detectionRadius;
    public float GetDamage() => 0f;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;
    public float GetCurrentHealth() => health ? health.GetCurrentHealth() : 0f;

    // ===== IMeleeHost (para MeleeAttackState) =====
    public Transform Transform => transform;
    public Animator Animator => animator;
    public Rigidbody2D Body => rb;
    public EnemyAttack Attack => attack;

    // ===== Helpers =====
    public bool IsMeleeing() => fsm.GetCurrentState() is MeleeAttackState;
    public bool CanMeleeAttack() => Time.time >= nextMeleeAllowedTime;
    public void MarkMeleeUsed() => nextMeleeAllowedTime = Time.time + meleeCooldown;
    public bool IsPlayerInMeleeRange()
    {
        if (!player) return false;
        return Vector2.Distance(transform.position, player.position) <= meleeAttackRange;
    }
    public bool IsFollowing() => fsm.GetCurrentState() is EnemyFollowState;

    // Death (idéntico patrón al Golem)
    public void RegisterDeathState(EnemyDeathState s) => _deathRef = s;
    public void OnDeathAnimFinished() => _deathRef?.OnDeathAnimFinished();

    // Melee (idéntico patrón al Golem)
    public void RegisterMeleeState(MeleeAttackState s) => _meleeRef = s;
    public void OnMeleeHit() => _meleeRef?.OnMeleeHit();
    public void OnMeleeFinished() => _meleeRef?.OnMeleeFinished();

    public bool IsDormant() => fsm.GetCurrentState() is MimicoDormantState;
    public bool IsAwakening() => fsm.GetCurrentState() is MimicoAwakenState;

    public void RegisterAwakenState(MimicoAwakenState s) => _awakenRef = s;
    public void OnAwakenFinished() => _awakenRef?.OnAwakenFinished();

    public Sprite InteractIcon => interactIcon;
    public float SpriteOffsetY => spriteOffsetY;
    public string IconSortingLayer => iconSortingLayer;
    public int IconSortingOrder => iconSortingOrder;

    private SpriteRenderer runtimeIconRenderer;

    public void SetBossCollidersEnabled(bool enabled)
    {
        if (toggleAllColliders)
        {
            var all = GetComponents<Collider2D>();
            foreach (var c in all) if (c) c.enabled = enabled;
        }
        else
        {
            foreach (var c in collidersToToggle) if (c) c.enabled = enabled;
        }
    }

    // ===== Icono “F” solo en Dormant =====
    public void EnsureDormantIcon()
    {
        if (runtimeIconRenderer == null)
        {
            var go = new GameObject("InteractIcon");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0f, spriteOffsetY, 0f);
            runtimeIconRenderer = go.AddComponent<SpriteRenderer>();
            runtimeIconRenderer.sortingLayerName = iconSortingLayer;
            runtimeIconRenderer.sortingOrder = iconSortingOrder;
        }
        runtimeIconRenderer.sprite = interactIcon;
        runtimeIconRenderer.enabled = interactIcon != null;
    }

    public void SetDormantIconVisible(bool v)
    {
        if (runtimeIconRenderer != null) runtimeIconRenderer.enabled = v && interactIcon != null;
    }

    public void DestroyDormantIcon()
    {
        if (runtimeIconRenderer != null)
        {
            Destroy(runtimeIconRenderer.gameObject);
            runtimeIconRenderer = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }
}