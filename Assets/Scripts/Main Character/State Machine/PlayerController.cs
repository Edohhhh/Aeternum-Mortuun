using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [HideInInspector] public bool canMove = true;
    public float moveSpeed = 90f;

    [Header("Combate")]
    public int baseDamage = 1;

    [Header("Dash")]
    public float dashSpeed = 300f;
    public int dashIframes = 10;
    public float dashSlideDuration = 0.1f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.75f;

    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody2D rb;
    public Collider2D hitbox;
    [HideInInspector] public StateMachine stateMachine;

    [Header("PowerUps")]
    public PowerUp[] initialPowerUps;

    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public DashState DashState { get; private set; }
    public KnockbackState KnockbackState { get; private set; }

    private Vector2 moveInput;
    private Vector2 lastNonZeroMoveInput = Vector2.right; // se actualiza solo cuando NO hay dash
    private float dashCooldownTimer;
    public bool isInvulnerable { get; set; }

    // Dash flags / dir
    [HideInInspector] public bool IsDashing { get; set; }
    public Vector2 RequestedDashDir { get; private set; } = Vector2.right;

    // ====== Ataque (Hitbox por Animation Event - opcional) ======
    [Header("Ataque (Hitbox - opcional por Animation Event)")]
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float hitboxLifetime = 0.10f;
    [SerializeField] private float knockbackForce = 100f;
    [SerializeField] private bool spawnHitboxViaAnimationEvent = false;
    private Vector2 lastAimDir = Vector2.right;
    // ============================================================

    // ====== Knockback sensible ======
    [Header("Knockback (Player)")]
    [Tooltip("Decaimiento del knockback (unidades de velocidad por segundo).")]
    public float knockbackDecay = 18f;
    [Tooltip("Límite superior de velocidad por knockback (magnitud).")]
    public float knockbackMaxSpeed = 22f;
    [Tooltip("Ventana (s) para apilar impulsos si hay golpes muy rápidos.")]
    public float knockbackStackWindow = 0.08f;
    [Tooltip("Si true, el knockback corta el dash al impactar.")]
    public bool knockbackInterruptsDash = true;
    [Tooltip("Si true, el knockback corta el recoil del ataque.")]
    public bool knockbackInterruptsRecoil = true;

    private Vector2 knockVel;          // velocidad acumulada por knockback
    private float knockWindowTimer;    // ventana para apilar hits
    private bool knockActive;          // hay knockback en curso
    // =================================

    // Ref al CombatSystem para coordinar con recoil
    private CombatSystem combat;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        combat = GetComponent<CombatSystem>();
        stateMachine = new StateMachine();

        IdleState = new IdleState(this, stateMachine);
        MoveState = new MoveState(this, stateMachine);
        DashState = new DashState(this, stateMachine);
        KnockbackState = new KnockbackState(this, stateMachine);

        dashCooldownTimer = 0f;
    }

    private void Start()
    {
        stateMachine.Initialize(IdleState);

        foreach (var powerUp in initialPowerUps)
        {
            if (powerUp != null)
                powerUp.Apply(this);
        }
    }

    private void Update()
    {
        // Flip sprite hacia el cursor
        Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = mousePos.x < playerScreenPos.x;

        // Dash cooldown
        dashCooldownTimer -= Time.deltaTime;

        // Input de movimiento (solo si puede moverse)
        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(moveX, moveY).normalized;

            // ⭐ Guardar la última dirección NO cero SOLO cuando NO estoy dashing
            if (!IsDashing && moveInput.sqrMagnitude > 0.0001f)
                lastNonZeroMoveInput = moveInput;
        }

        // Iniciar dash usando input actual o el último guardado
        if (Input.GetButtonDown("Jump") &&
            dashCooldownTimer <= 0f &&
            canMove &&
            (moveInput.sqrMagnitude > 0.0001f || lastNonZeroMoveInput.sqrMagnitude > 0.0001f))
        {
            dashCooldownTimer = dashCooldown;
            RequestedDashDir = (moveInput.sqrMagnitude > 0.0001f ? moveInput : lastNonZeroMoveInput).normalized;
            stateMachine.ChangeState(DashState);
            return;
        }

        // Si no puede moverse, frenar y salir
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            moveInput = Vector2.zero;
            if (animator != null) animator.SetBool("isMoving", false);
            return;
        }

        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        // ⛔ PRIORIDAD 1: Knockback (si está activo, nadie más escribe la velocidad)
        if (knockActive)
        {
            // Aplicar velocidad de knockback
            rb.linearVelocity = knockVel;

            // Decaimiento suave hacia 0
            knockVel = Vector2.MoveTowards(knockVel, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
            if (knockWindowTimer > 0f) knockWindowTimer -= Time.fixedDeltaTime;

            // Cierre del knockback
            if (knockVel.sqrMagnitude < 0.0001f && knockWindowTimer <= 0f)
            {
                knockActive = false;
                rb.linearVelocity = Vector2.zero;
                canMove = true; // liberar control (si nadie más bloquea)
            }
            return;
        }

        // ⛔ PRIORIDAD 2: Recoil bloqueado (no dejar que la FSM mueva)
        bool recoilBloqueado = (combat != null && combat.IsRecoiling() && !combat.allowMovementDuringAttack);
        if (!canMove || recoilBloqueado)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // FSM normal
        stateMachine.CurrentState.PhysicsUpdate();
    }

    public Vector2 GetMoveInput() => moveInput;

    // ====== API PÚBLICA: aplicar knockback al Player ======
    public void ApplyKnockback(Vector2 direction, float strength)
    {
        if (strength <= 0f) return;

        // Interrupciones opcionales
        if (knockbackInterruptsDash && IsDashing)
        {
            // Volvé a Idle; tu DashState hará su cleanup en Exit()
            stateMachine.ChangeState(IdleState);
        }
        if (knockbackInterruptsRecoil && combat != null)
        {
            combat.ForceStopRecoil(); // corta el micro-dash del ataque
        }

        // Bloquear movimiento del jugador (stun leve)
        canMove = false;

        // Acumular impulso durante una pequeña ventana
        Vector2 impulse = direction.sqrMagnitude > 0.0001f ? direction.normalized * strength : Vector2.zero;
        knockVel += impulse;
        knockVel = Vector2.ClampMagnitude(knockVel, knockbackMaxSpeed);
        knockWindowTimer = knockbackStackWindow;

        knockActive = true;
    }
    // ======================================================

    // ====== Hitbox por Animation Event (opcional) ======
    public void SpawnAttackHitbox()
    {
        if (!spawnHitboxViaAnimationEvent) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = lastAimDir;

        Vector2 spawnPos = (Vector2)attackOrigin.position + dir * attackRange;

        var go = Instantiate(hitboxPrefab, spawnPos, Quaternion.identity, transform);
        var hb = go.GetComponentInChildren<AttackHitbox>(true);
        if (hb == null) { Debug.LogError("AttackHitbox no está en el root ni en hijos del prefab."); return; }

        hb.damage = Mathf.Max(1, baseDamage);
        hb.knockbackDir = dir;
        hb.knockbackForce = knockbackForce;
        hb.lifeTime = hitboxLifetime;

        lastAimDir = dir;
    }
    // ====================================================

    private Vector2 GetAimDirection()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f)
            dir = lastAimDir;

        lastAimDir = dir;
        return dir;
    }

    // ====== Persistencia ======
    public void SavePlayerData()
    {
        GameDataManager.Instance.SavePlayerData(this);
    }

    public void LoadPlayerData()
    {
        var data = GameDataManager.Instance.playerData;

        moveSpeed = data.moveSpeed;
        dashSpeed = data.dashSpeed;
        dashIframes = data.dashIframes;
        dashSlideDuration = data.dashSlideDuration;
        dashDuration = data.dashDuration;
        dashCooldown = data.dashCooldown;

        baseDamage = (data.baseDamage > 0) ? data.baseDamage : baseDamage;

        transform.position = data.position;

        var health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.maxHealth = data.maxHealth;
            health.currentHealth = data.currentHealth;
            health.regenerationRate = data.regenerationRate;
            health.regenDelay = data.regenDelay;
            health.invulnerableTime = data.invulnerableTime;
            health.UpdateUI();
        }

        initialPowerUps = data.initialPowerUps.ToArray();
        foreach (var powerUp in initialPowerUps)
        {
            if (powerUp != null)
                powerUp.Apply(this);
        }
    }
}
