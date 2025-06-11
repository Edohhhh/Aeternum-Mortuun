using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [HideInInspector] public bool canMove = true;
    public float moveSpeed = 90f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    [Range(0f, 1f)] public float slideFactor = 0.15f;

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

    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public DashState DashState { get; private set; }
    public KnockbackState KnockbackState { get; private set; }

    private Vector2 moveInput;
    private float dashCooldownTimer;

    public bool isInvulnerable { get; set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
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
    }

    void Update()
    {
        // 🔥 Flip del sprite para mirar al cursor
        Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;

        if (mousePos.x < playerScreenPos.x)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        // 🔥 Dash cooldown
        dashCooldownTimer -= Time.deltaTime;

        // 🔥 Obtén input de movimiento solo si no está bloqueado
        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(moveX, moveY).normalized;
        }

        // 🔥 Inicia dash (usa StateMachine)
        if (Input.GetButtonDown("Jump") && dashCooldownTimer <= 0f && moveInput != Vector2.zero)
        {
            dashCooldownTimer = dashCooldown;
            stateMachine.ChangeState(DashState);
            return; // 🔥 No sigas con otros estados en el mismo frame
        }

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetBool("isMoving", false);
            return;
        }

        // 🔥 Usa la StateMachine normalmente
        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState.PhysicsUpdate();
    }

    public Vector2 GetMoveInput() => moveInput;

    public void SavePlayerData()
    {
        GameDataManager.Instance.SavePlayerData(this);
    }

    public void LoadPlayerData()
    {
        var data = GameDataManager.Instance.playerData;

        moveSpeed = data.moveSpeed;
        acceleration = data.acceleration;
        deceleration = data.deceleration;
        slideFactor = data.slideFactor;
        dashSpeed = data.dashSpeed;
        dashIframes = data.dashIframes;
        dashSlideDuration = data.dashSlideDuration;
        dashDuration = data.dashDuration;
        dashCooldown = data.dashCooldown;

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
    }
}
