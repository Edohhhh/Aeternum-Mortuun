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
    public Vector2 lastNonZeroMoveInput = Vector2.right;
    private float dashCooldownTimer;
    public bool isInvulnerable { get; set; }

    [HideInInspector] public bool IsDashing { get; set; }
    public Vector2 RequestedDashDir { get; private set; } = Vector2.right;

    // ===== Ataque por Animation Event (opcional) =====
    [Header("Ataque (Hitbox - opcional por Animation Event)")]
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float hitboxLifetime = 0.10f;
    [SerializeField] private float knockbackForce = 100f;
    [SerializeField] private bool spawnHitboxViaAnimationEvent = false;
    private Vector2 lastAimDir = Vector2.right;

    // ===== Knockback =====
    [Header("Knockback (Player)")]
    public float knockbackDecay = 18f;
    public float knockbackMaxSpeed = 22f;
    public float knockbackStackWindow = 0.08f;
    public bool knockbackInterruptsDash = true;
    public bool knockbackInterruptsRecoil = true;

    private Vector2 knockVel;
    private float knockWindowTimer;
    private bool knockActive;

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

        dashCooldownTimer -= Time.deltaTime;

        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(moveX, moveY).normalized;

            if (!IsDashing && moveInput.sqrMagnitude > 0.0001f)
                lastNonZeroMoveInput = moveInput;
        }

        // 🚫 Bloqueo de dash si estoy atacando
        if (Input.GetButtonDown("Jump") &&
            dashCooldownTimer <= 0f &&
            canMove &&
            !combat.IsRecoiling() &&
            !IsDashing &&
            (moveInput.sqrMagnitude > 0.0001f || lastNonZeroMoveInput.sqrMagnitude > 0.0001f))
        {
            dashCooldownTimer = dashCooldown;
            RequestedDashDir = (moveInput.sqrMagnitude > 0.0001f ? moveInput : lastNonZeroMoveInput).normalized;
            stateMachine.ChangeState(DashState);
            return;
        }

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
        if (knockActive)
        {
            rb.linearVelocity = knockVel;
            knockVel = Vector2.MoveTowards(knockVel, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
            if (knockWindowTimer > 0f) knockWindowTimer -= Time.fixedDeltaTime;

            if (knockVel.sqrMagnitude < 0.0001f && knockWindowTimer <= 0f)
            {
                knockActive = false;
                rb.linearVelocity = Vector2.zero;
                canMove = true;
            }
            return;
        }

        bool recoilBloqueado = (combat != null && combat.IsRecoiling() && !combat.allowMovementDuringAttack);
        if (!canMove || recoilBloqueado)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        stateMachine.CurrentState.PhysicsUpdate();
    }

    public Vector2 GetMoveInput() => moveInput;

    public void ApplyKnockback(Vector2 direction, float strength)
    {
        if (strength <= 0f) return;

        if (knockbackInterruptsDash && IsDashing)
            stateMachine.ChangeState(IdleState);

        if (knockbackInterruptsRecoil && combat != null)
            combat.ForceStopRecoil();

        canMove = false;

        Vector2 impulse = direction.sqrMagnitude > 0.0001f ? direction.normalized * strength : Vector2.zero;
        knockVel += impulse;
        knockVel = Vector2.ClampMagnitude(knockVel, knockbackMaxSpeed);
        knockWindowTimer = knockbackStackWindow;

        knockActive = true;
    }

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
