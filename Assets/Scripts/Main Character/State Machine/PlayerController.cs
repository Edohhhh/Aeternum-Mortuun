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

    // Estados
    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public DashState DashState { get; private set; }
    public RecoilState RecoilState { get; private set; }
    public KnockbackState KnockbackState { get; private set; }

    private Vector2 moveInput;
    public Vector2 lastNonZeroMoveInput = Vector2.right;
    [HideInInspector] public float dashCooldownTimer;
    public bool isInvulnerable { get; set; }

    [HideInInspector] public bool IsDashing { get; set; }
    public Vector2 RequestedDashDir { get; private set; } = Vector2.right;

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

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();

        IdleState = new IdleState(this, stateMachine);
        MoveState = new MoveState(this, stateMachine);
        DashState = new DashState(this, stateMachine);
        RecoilState = new RecoilState(this, stateMachine);
        KnockbackState = new KnockbackState(this, stateMachine);

        dashCooldownTimer = 0f;
    }

    private void Start()
    {
        stateMachine.Initialize(IdleState);

        foreach (var powerUp in initialPowerUps)
            if (powerUp != null) powerUp.Apply(this);
    }

    private void Update()
    {
        dashCooldownTimer -= Time.deltaTime;

        // 🚫 Bloquear TODO input mientras dure el recoil
        if (stateMachine.CurrentState == RecoilState)
        {
            stateMachine.CurrentState.LogicUpdate();
            return;
        }

        // Flip sprite hacia el cursor
        Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = mousePos.x < playerScreenPos.x;

        // Leer input de movimiento
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (!IsDashing && moveInput.sqrMagnitude > 0.0001f)
            lastNonZeroMoveInput = moveInput;

        // Dash (solo si no estoy en recoil ni ya dashing)
        if (Input.GetButtonDown("Jump") &&
            dashCooldownTimer <= 0f &&
            canMove &&
            !IsDashing &&
            (moveInput.sqrMagnitude > 0.0001f || lastNonZeroMoveInput.sqrMagnitude > 0.0001f))
        {
            // 🔥 Reiniciar cooldown al APRETAR dash
            dashCooldownTimer = dashCooldown;

            RequestedDashDir = (moveInput.sqrMagnitude > 0.0001f ? moveInput : lastNonZeroMoveInput).normalized;
            stateMachine.ChangeState(DashState);
        }

        // FSM
        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();

        // Animación opcional
        if (animator != null)
        {
            bool bloqueado = stateMachine.CurrentState == RecoilState ||
                             stateMachine.CurrentState == KnockbackState;
            animator.SetBool("isMoving", !bloqueado && canMove && moveInput.sqrMagnitude > 0.0001f);
        }
    }

    private void FixedUpdate()
    {
        // Knockback por fuerza acumulada
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
        }

        // FSM siempre activa su PhysicsUpdate
        stateMachine.CurrentState.PhysicsUpdate();
    }

    public Vector2 GetMoveInput() => moveInput;

    public void ApplyKnockback(Vector2 direction, float strength)
    {
        if (strength <= 0f) return;

        if (knockbackInterruptsDash && IsDashing)
            stateMachine.ChangeState(IdleState);

        if (knockbackInterruptsRecoil && stateMachine.CurrentState == RecoilState)
            stateMachine.ChangeState(IdleState);

        canMove = false;

        Vector2 impulse = direction.sqrMagnitude > 0.0001f ? direction.normalized * strength : Vector2.zero;
        knockVel += impulse;
        knockVel = Vector2.ClampMagnitude(knockVel, knockbackMaxSpeed);
        knockWindowTimer = knockbackStackWindow;

        knockActive = true;
    }

    // ==== Guardado y carga de datos del jugador ====
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
            if (powerUp != null) powerUp.Apply(this);
    }
}
