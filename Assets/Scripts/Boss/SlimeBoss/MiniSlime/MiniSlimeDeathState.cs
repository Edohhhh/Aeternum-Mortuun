using UnityEngine;

public class MiniSlimeDeathState : State<EnemyInputs>
{
    private MiniSlimeController slime;
    private float deathDelay = 0.5f;
    private float timer = 0f;

    public MiniSlimeDeathState(MiniSlimeController slime)
    {
        this.slime = slime;
    }

    public override void Awake()
    {
        base.Awake();
        timer = 0f;
    }

    public override void Execute()
    {
        timer += Time.deltaTime;
        if (timer >= deathDelay)
        {
            slime.Die();
        }
    }

    public override void Sleep() { }
}
