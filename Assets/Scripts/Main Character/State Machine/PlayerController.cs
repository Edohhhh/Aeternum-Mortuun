using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [HideInInspector] public bool canMove = true;
    public float moveSpeed = 3.5f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    [Range(0f, 1f)] public float slideFactor = 0.15f;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public int dashIframes = 10;
    public float dashSlideDuration = 0.1f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Attack")]
    public GameObject attackPrefab;
    public float attackDuration = 0.5f;

    // Components
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody2D rb;
    public Collider2D hitbox; // BoxCollider2D for damage/collision
    [HideInInspector] public StateMachine stateMachine;

    // State instances
    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public DashState DashState { get; private set; }
    public AttackState AttackState { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new StateMachine();

        IdleState = new IdleState(this, stateMachine);
        MoveState = new MoveState(this, stateMachine);
        DashState = new DashState(this, stateMachine);
        AttackState = new AttackState(this, stateMachine);
    }

    private void Start()
    {
        stateMachine.Initialize(IdleState);
    }

    void Update()
    {
        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.LogicUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.CurrentState.PhysicsUpdate();
    }
}
