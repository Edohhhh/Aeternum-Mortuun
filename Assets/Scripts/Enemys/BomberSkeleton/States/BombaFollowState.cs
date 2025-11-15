using UnityEngine;

public class BombaFollowState : State<EnemyInputs>
{
    private readonly IEnemyDataProvider data;
    private readonly Rigidbody2D rb;
    private readonly Animator animator;
    private readonly Transform enemyTransform;
    private Vector2 currentVelocity = Vector2.zero;

    // Constructor de 4 argumentos para BombaController
    public BombaFollowState(IEnemyDataProvider dataProvider, Animator anim, Rigidbody2D rigidBody, Transform transform)
    {
        this.data = dataProvider;
        this.animator = anim;
        this.rb = rigidBody;
        this.enemyTransform = transform;
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
        enemyTransform.rotation = Quaternion.identity;
    }

    public override void FixedExecute()
    {
        if (data == null || data.GetPlayer() == null || rb == null) return;

        Vector2 toPlayer = (Vector2)data.GetPlayer().position - (Vector2)enemyTransform.position;
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