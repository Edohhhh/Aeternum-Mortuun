
using UnityEngine;

public class EnemyFollowState : State<EnemyInputs>
{
    private readonly Transform enemy;
    private readonly IEnemyDataProvider data;
    private readonly Rigidbody2D rb;
    private Vector2 currentVelocity = Vector2.zero;
    private Animator animator;

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
        if (animator != null)
            animator.SetBool("isWalking", true);
    }

    public override void Execute()
    {
        if (data == null || data.GetPlayer() == null) return;

        Vector2 direction = (data.GetPlayer().position - enemy.position).normalized;
        currentVelocity = direction * data.GetMaxSpeed();
        rb.MovePosition(rb.position + currentVelocity * Time.deltaTime);

        // **Esto es lo nuevo**: si alguna animación o golpe te ha girado,
        // lo reseteamos cada frame a 0º:
        enemy.rotation = Quaternion.identity;
    }

    public override void Sleep()
    {
        base.Sleep();
        // Desactivamos la animación de caminar
        if (animator != null)
            animator.SetBool("isWalking", false);
    }
}
