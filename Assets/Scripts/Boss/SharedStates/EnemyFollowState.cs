using UnityEngine;

public class EnemyFollowState : State<EnemyInputs>
{
    private readonly Transform enemy;
    private readonly IEnemyDataProvider data;
    private readonly Rigidbody2D rb;
    private readonly Animator animator;
    private Vector2 currentVelocity = Vector2.zero;

    // Este es el constructor de 3 argumentos que usan tus otros enemigos
    public EnemyFollowState(Transform enemy, Transform player, float speed)
    {
        this.enemy = enemy;
        this.data = enemy.GetComponent<IEnemyDataProvider>();
        this.rb = enemy.GetComponent<Rigidbody2D>();
        this.animator = enemy.GetComponent<Animator>();
    }

    public override void Awake()
    {
        base.Awake();
        if (animator) animator.SetBool("isWalking", true);
        if (rb)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public override void Execute()
    {
        if (data == null || data.GetPlayer() == null) return;
        enemy.rotation = Quaternion.identity;
    }

    public override void FixedExecute()
    {
        if (data == null || data.GetPlayer() == null || rb == null) return;

        Vector2 toPlayer = (Vector2)data.GetPlayer().position - (Vector2)enemy.position;
        Vector2 dir = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;
        Vector2 desiredVel = dir * data.GetMaxSpeed();
        float accel = Mathf.Max(0f, data.GetAcceleration());

        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVel,
            accel * Time.fixedDeltaTime
        );

        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
    }

    public override void Sleep()
    {
        if (animator) animator.SetBool("isWalking", false);
        base.Sleep();
    }
}