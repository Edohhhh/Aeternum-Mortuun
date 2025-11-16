using UnityEngine;
using System.Collections.Generic; // Necesario para la lógica de PowerUpEffect

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [HideInInspector] public bool canMove = true;
    public float moveSpeed = 90f;

    [Header("Combate")]
    // ✅ --- VALOR CAMBIADO ---
    public int baseDamage = 3;

    // ✅ --- AÑADIDO ---
    [Header("Stats de Ruleta")]
    [Tooltip("Cuántas tiradas extra tiene el jugador (stackeable)")]
    public int extraSpins = 0;
    // ✅ --- FIN ---

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

    // Knockback
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

        // Este Start() se ejecuta ANTES de LoadPlayerData,
        // así que los 'initialPowerUps' aquí son solo los de "debug"
        // o los de una partida nueva. LoadPlayerData los sobreescribirá.
        foreach (var powerUp in initialPowerUps)
            if (powerUp != null) powerUp.Apply(this);
    }

    private void Update()
    {
        dashCooldownTimer -= Time.deltaTime;

        // 🔒 Bloquear todo input mientras dure el RecoilState (ataque)
        if (stateMachine.CurrentState == RecoilState)
        {
            stateMachine.CurrentState.LogicUpdate();
            return;
        }

        // Flip sprite hacia el cursor
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 mousePos = Input.mousePosition;
            sr.flipX = mousePos.x < playerScreenPos.x;
        }

        // Input movimiento
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (!IsDashing && moveInput.sqrMagnitude > 0.0001f)
            lastNonZeroMoveInput = moveInput;

        // DASH: sólo desde acá, respetando cooldown y sin permitirlo en RecoilState
        if (Input.GetButtonDown("Jump") &&
            dashCooldownTimer <= 0f &&
            canMove &&
            !IsDashing &&
            stateMachine.CurrentState != RecoilState &&
            (moveInput.sqrMagnitude > 0.0001f || lastNonZeroMoveInput.sqrMagnitude > 0.0001f))
        {
            dashCooldownTimer = dashCooldown; // fija cooldown al apretar
            RequestedDashDir = (moveInput.sqrMagnitude > 0.0001f ? moveInput : lastNonZeroMoveInput).normalized;
            stateMachine.ChangeState(DashState);
            return; // evita doble procesamiento este frame
        }

        // FSM
        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();


        // Animator: NO marcar isMoving durante Dash/Knockback/Recoil/Attack
        if (animator != null)
        {
            bool enAtaque = animator.GetBool("isAttacking");
            bool bloqueado = stateMachine.CurrentState == RecoilState ||
                                stateMachine.CurrentState == KnockbackState ||
                                stateMachine.CurrentState == DashState ||
                                enAtaque;

            animator.SetBool("isMoving", !bloqueado && canMove && moveInput.sqrMagnitude > 0.0001f);
        }
    }

    private void FixedUpdate()
    {
        // Knockback acumulativo (bloquea physics de la FSM mientras dure)
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

        // Si no puedo moverme, no aplico movimiento
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // FSM physics
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

    // ✅ --- MÉTODO 'LoadPlayerData' COMPLETAMENTE REEMPLAZADO ---
    // (Esto es VITAL para que los power-ups stackeables funcionen
    // y no se acumulen infinitamente en cada carga de escena)
    public void LoadPlayerData()
    {
        var data = GameDataManager.Instance.playerData;

        // --- Stats de vida ---
        var health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.maxHealth = data.maxHealth;
            health.currentHealth = health.maxHealth; // full life
            health.regenerationRate = data.regenerationRate;
            health.regenDelay = data.regenDelay;
            health.invulnerableTime = data.invulnerableTime;

            if (health.healthUI != null)
            {
                health.healthUI.Initialize(health.maxHealth);
                health.healthUI.UpdateHearts(health.currentHealth);
            }
        }

        // ✅ --- RESETEAR STATS BASE ---
        // ¡VITAL! Resetea stats a su valor por defecto ANTES de
        // reaplicar los power-ups, para evitar que se stackeen.

        // ✅ --- VALOR CAMBIADO ---
        this.baseDamage = 3; // (O tu valor base por defecto)
        this.extraSpins = 0;
        // (Añade aquí cualquier otra stat que tus power-ups modifiquen)

        // --- Restaurar perks guardadas ---
        if (data.initialPowerUps != null && data.initialPowerUps.Count > 0)
        {
            // Crear un array nuevo con el tamaño justo
            initialPowerUps = data.initialPowerUps.ToArray();

            // Reaplicar perks al jugador
            foreach (var powerUp in initialPowerUps)
            {
                if (powerUp != null)
                    powerUp.Apply(this);
            }
        }
    }

}