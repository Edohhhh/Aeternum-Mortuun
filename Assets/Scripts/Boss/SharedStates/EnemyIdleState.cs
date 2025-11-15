using UnityEngine;

public class EnemyIdleState : State<EnemyInputs>
{
    private Transform enemy;
    private float idleTime;
    private float timer = 0f;
    private SpriteRenderer spriteRenderer;

    // Este es el constructor que usan tus Slimes (1 o 2 argumentos)
    public EnemyIdleState(Transform enemy, float idleTime = 2f)
    {
        this.enemy = enemy;
        this.idleTime = idleTime;
        spriteRenderer = enemy.GetComponent<SpriteRenderer>();
    }

    public override void Awake()
    {
        base.Awake();
        timer = 0f;

    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        if (timer >= idleTime)
        {
            // (Tu lógica original de timer)
        }
    }

    public override void Sleep()
    {

    }
}