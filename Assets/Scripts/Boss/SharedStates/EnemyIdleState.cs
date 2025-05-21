using UnityEngine;

public class EnemyIdleState : State<EnemyInputs>
{
    private Transform enemy;
    private float idleTime;
    private float timer = 0f;
    private SpriteRenderer spriteRenderer;

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
        if (spriteRenderer != null)
            spriteRenderer.color = Color.green;
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        if (timer >= idleTime)
        {
            
        }
    }

    public override void Sleep()
    {
        
    }
}
