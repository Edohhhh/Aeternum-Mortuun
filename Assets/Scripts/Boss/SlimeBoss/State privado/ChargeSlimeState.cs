using UnityEngine;

public class ChargeSlimeState : State<EnemyInputs>, ICollisionHandler
{
    private readonly SlimeController slime;
    private Transform enemy;
    private Rigidbody2D rb;
    private IEnemyDataProvider data;
    private FSM<EnemyInputs> fsm;
    private GameObject acidPrefab;

    private float dashSpeed = 15f;
    private float dashDuration = 8f;
    private float recoveryDuration = 0.5f;

    private float timer = 0f;
    private float acidSpawnTimer = 0f;
    private float acidSpawnInterval = 0.1f;

    private Vector2 dashDirection;
    private Vector2 currentDirection;

    private enum Phase { Dashing, Recovery }
    private Phase currentPhase = Phase.Dashing;

    public ChargeSlimeState(SlimeController slime, FSM<EnemyInputs> fsm, GameObject acidPrefab)
    {
        this.slime = slime;
        this.enemy = slime.transform;
        this.rb = enemy.GetComponent<Rigidbody2D>();
        this.data = slime;
        this.fsm = fsm;
        this.acidPrefab = acidPrefab;
    }

    public override void Awake()
    {
        base.Awake();
        timer = 0f;
        acidSpawnTimer = 0f;
        currentPhase = Phase.Dashing;

        if (data.GetPlayer() != null)
            dashDirection = (data.GetPlayer().position - enemy.position).normalized;
        else
            dashDirection = Vector2.right;

        currentDirection = dashDirection;

        Debug.Log("ChargeState: Inició dash real.");
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        if (currentPhase == Phase.Dashing)
        {
            rb.linearVelocity = currentDirection * dashSpeed;
            acidSpawnTimer += Time.deltaTime;

            if (acidSpawnTimer >= acidSpawnInterval)
            {
                acidSpawnTimer = 0f;
                GameObject.Instantiate(acidPrefab, enemy.position, Quaternion.identity);
            }

            if (timer >= dashDuration)
            {
                rb.linearVelocity = Vector2.zero;
                timer = 0f;
                currentPhase = Phase.Recovery;
                Debug.Log("ChargeState: Fin del dash, entrando en recuperación.");
            }
        }
        else if (currentPhase == Phase.Recovery)
        {
            rb.linearVelocity = Vector2.zero;

            if (timer >= recoveryDuration)
            {
                var next = data.GetPlayer() != null ? EnemyInputs.SeePlayer : EnemyInputs.LostPlayer;
                fsm.Transition(next);
                Debug.Log("ChargeState: Fin de recuperación.");
            }
        }
    }

    // --- Aquí rebota ---
    public void OnCollisionEnter2D(Collision2D collision)
    {

        if (!collision.gameObject.CompareTag("Wall"))
            return;

        if (collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            currentDirection = Vector2.Reflect(currentDirection, normal).normalized;

            Debug.Log($"ChargeState: Rebote con normal {normal}, nueva dirección {currentDirection}");
        }
    }

    public override void Sleep()
    {
        slime.MarkSpecialUsed();
        rb.linearVelocity = Vector2.zero;
        base.Sleep();
    }
}
