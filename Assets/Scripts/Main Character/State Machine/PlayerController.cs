using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [HideInInspector] public bool canMove = true;
    public float moveSpeed = 90f;

    [Header("Combate")]
    public int baseDamage = 3;

    [Header("Stats de Ruleta")]
    public int extraSpins = 0;

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
    public AttackState AttackState { get; private set; } // ✅ NUEVO ESTADO
    public KnockbackState KnockbackState { get; private set; }

    private Vector2 moveInput;
    public Vector2 lastNonZeroMoveInput = Vector2.right;
    [HideInInspector] public float dashCooldownTimer;
    public bool isInvulnerable { get; set; }
    [HideInInspector] public bool IsDashing { get; set; }
    public Vector2 RequestedDashDir { get; set; } = Vector2.right;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();

        // Inicialización de Estados
        IdleState = new IdleState(this, stateMachine);
        MoveState = new MoveState(this, stateMachine);
        DashState = new DashState(this, stateMachine);
        AttackState = new AttackState(this, stateMachine); // ✅ Instanciar AttackState
        KnockbackState = new KnockbackState(this, stateMachine);

        dashCooldownTimer = 0f;
    }

    private void Start()
    {
        stateMachine.Initialize(IdleState);

        // Aplicar PowerUps iniciales
        if (initialPowerUps != null)
        {
            foreach (var powerUp in initialPowerUps)
            {
                if (powerUp != null)
                {
                    powerUp.Apply(this);
                }
            }
        }
    }

    private void Update()
    {
        dashCooldownTimer -= Time.deltaTime;

        // 1. Prioridad Knockback
        if (stateMachine.CurrentState == KnockbackState)
        {
            stateMachine.CurrentState.LogicUpdate();
            return;
        }

        // Flip Sprite
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && canMove && !IsDashing)
        {
            Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 mousePos = Input.mousePosition;
            sr.flipX = mousePos.x < playerScreenPos.x;
        }

        // Input Movimiento
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;

        if (!IsDashing && moveInput.sqrMagnitude > 0.0001f)
            lastNonZeroMoveInput = moveInput;

        // 2. Input Ataque (Entrada al estado)
        if (Input.GetButtonDown("Fire1") && canMove && !IsDashing)
        {
            if (stateMachine.CurrentState != AttackState)
            {
                stateMachine.ChangeState(AttackState);
            }
        }

        // 3. Input Dash
        if (Input.GetButtonDown("Jump") &&
            dashCooldownTimer <= 0f &&
            canMove &&
            !IsDashing &&
            (moveInput.sqrMagnitude > 0.0001f || lastNonZeroMoveInput.sqrMagnitude > 0.0001f))
        {
            dashCooldownTimer = dashCooldown;
            RequestedDashDir = (moveInput.sqrMagnitude > 0.0001f ? moveInput : lastNonZeroMoveInput).normalized;
            stateMachine.ChangeState(DashState);
            return;
        }

        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();

        // Actualizar Animador
        if (animator != null)
        {
            bool actionState = stateMachine.CurrentState == AttackState ||
                               stateMachine.CurrentState == KnockbackState ||
                               stateMachine.CurrentState == DashState;

            animator.SetBool("isMoving", !actionState && canMove && moveInput.sqrMagnitude > 0.0001f);
        }
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState.PhysicsUpdate();
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (isInvulnerable) return;

        KnockbackState.SetKnockback(direction, force, duration);
        stateMachine.ChangeState(KnockbackState);
    }

    // ✅ MÉTODO DE PUENTE PARA GUARDAR (Soluciona errores en SceneChanger y Portales)
    public void SavePlayerData()
    {
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.SavePlayerData(this);
            Debug.Log("[PlayerController] Datos guardados.");
        }
    }

    public void LoadPlayerData(PlayerData data)
    {
        transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);

        var health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.maxHealth = data.maxHealth;
            health.currentHealth = health.maxHealth;
            health.regenerationRate = data.regenerationRate;
            health.regenDelay = data.regenDelay;
            health.invulnerableTime = data.invulnerableTime;

            if (health.healthUI != null)
            {
                health.healthUI.Initialize(health.maxHealth);
                health.healthUI.UpdateHearts(health.currentHealth);
            }
        }

        this.baseDamage = 3;
        this.extraSpins = 0;

        if (data.initialPowerUps != null && data.initialPowerUps.Count > 0)
        {
            initialPowerUps = data.initialPowerUps.ToArray();
            foreach (var powerUp in initialPowerUps)
            {
                if (powerUp != null)
                    powerUp.Apply(this);
            }
        }
    }
    public void OnAttackHitEnemy()
    {
        // Verificamos si el estado actual es AttackState
        if (stateMachine.CurrentState == AttackState)
        {
            // Llamamos a la función de retroceso
            AttackState.ApplyHitRecoil();
        }
    }
    public Vector2 GetMoveInput() => moveInput;
}