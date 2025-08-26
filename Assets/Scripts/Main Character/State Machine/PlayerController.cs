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
    private float dashCooldownTimer;

    public bool isInvulnerable { get; set; }

    // ====== NUEVO: configuración de ataque / hitbox ======
    [Header("Ataque (Hitbox)")]
    [SerializeField] private GameObject hitboxPrefab;  // el prefab puede tener el AttackHitbox en un hijo
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float hitboxLifetime = 0.10f;
    [SerializeField] private float knockbackForce = 100f;

    private Vector2 lastAimDir = Vector2.right;
    // =====================================================

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

        foreach (var powerUp in initialPowerUps)
        {
            if (powerUp != null)
                powerUp.Apply(this);
        }
    }

    void Update()
    {
        // Flip sprite hacia el cursor
        Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;
        GetComponent<SpriteRenderer>().flipX = mousePos.x < playerScreenPos.x;

        // Dash cooldown
        dashCooldownTimer -= Time.deltaTime;

        // Input de movimiento
        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(moveX, moveY).normalized;
        }

        // Iniciar dash
        if (Input.GetButtonDown("Jump") && dashCooldownTimer <= 0f && moveInput != Vector2.zero)
        {
            dashCooldownTimer = dashCooldown;
            stateMachine.ChangeState(DashState);
            return;
        }

        if (!canMove)
        {
            // Nota: en Rigidbody2D la propiedad estándar es 'velocity'
            rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetBool("isMoving", false);
            return;
        }

        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();

        // (Opcional) Test rápido sin animación:
        // if (Input.GetButtonDown("Fire1")) SpawnAttackHitbox();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState.PhysicsUpdate();
    }

    public Vector2 GetMoveInput() => moveInput;

    // ====== NUEVO: Spawner de la hitbox (llamar desde Animation Event en el frame de impacto) ======


    public void SpawnAttackHitbox()
    {
        // Dirección ejemplo (mouse)
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;

        Vector2 spawnPos = (Vector2)attackOrigin.position + dir * attackRange;

        // 👇 Parentéalo al Player al instanciar, así ya nace como hijo del player
        var go = Instantiate(hitboxPrefab, spawnPos, Quaternion.identity, transform);

        // 👇 BUSCÁ EN HIJOS el componente AttackHitbox
        var hb = go.GetComponentInChildren<AttackHitbox>(true);
        if (hb == null) { Debug.LogError("AttackHitbox no está en el root ni en hijos del prefab."); return; }

        // Snapshot del daño actual del player
        hb.damage = Mathf.Max(1, baseDamage);
        hb.knockbackDir = dir;
        hb.knockbackForce = knockbackForce;
        hb.lifeTime = hitboxLifetime;
    }

    private Vector2 GetAimDirection()
    {
        // Apunta al mouse (top-down). Cambiá por la lógica que prefieras (input, mira, etc.)
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;

        if (dir.sqrMagnitude < 0.0001f)
            dir = lastAimDir;

        lastAimDir = dir;
        return dir;
    }
    // =================================================================================================

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

        // Cargar daño persistido
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

        // PowerUps (referencias directas)
        initialPowerUps = data.initialPowerUps.ToArray();
        foreach (var powerUp in initialPowerUps)
        {
            if (powerUp != null)
                powerUp.Apply(this);
        }
    }
}
