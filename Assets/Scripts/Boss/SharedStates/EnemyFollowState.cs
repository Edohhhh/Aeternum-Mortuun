
using UnityEngine;

public class EnemyFollowState : State<EnemyInputs>
{
    private readonly Transform enemy;
    private readonly IEnemyDataProvider data;
    private readonly Rigidbody2D rb;
    private readonly Animator animator;

    // velocidad actual integrada (para acelerar/suavizar)
    private Vector2 currentVelocity = Vector2.zero;

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

    // Lógica no física (opcional: mirar/flags)
    public override void Execute()
    {
        if (data == null || data.GetPlayer() == null) return;

        // Mantener rotación limpia; el flip lo maneja el controller
        enemy.rotation = Quaternion.identity;
    }

    // SOLO física: movimiento estable por fixedDeltaTime
    public override void FixedExecute()
    {
        if (data == null || data.GetPlayer() == null || rb == null) return;

        // dirección hacia el jugador
        Vector2 toPlayer = (Vector2)data.GetPlayer().position - (Vector2)enemy.position;
        Vector2 dir = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;

        // velocidad objetivo (magnitud = maxSpeed)
        Vector2 desiredVel = dir * data.GetMaxSpeed();

        // integrar con aceleración (suaviza cambios de dirección/arranque)
        float accel = Mathf.Max(0f, data.GetAcceleration());
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            desiredVel,
            accel * Time.fixedDeltaTime
        );

        // mover en paso fijo
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);
    }

    public override void Sleep()
    {
        if (animator) animator.SetBool("isWalking", false);
        base.Sleep();
    }
}
