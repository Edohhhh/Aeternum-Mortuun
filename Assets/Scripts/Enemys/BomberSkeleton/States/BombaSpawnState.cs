using UnityEngine;

public class BombaSpawnState : State<EnemyInputs>
{
    private readonly BombaController controller;
    private Rigidbody2D rb;
    private bool finished;

    public BombaSpawnState(BombaController ctrl)
    {
        controller = ctrl;
        rb = controller.GetComponent<Rigidbody2D>();
    }

    public override void Awake()
    {
        base.Awake();
        finished = false;
        controller.RegisterSpawnState(this);
        controller.GetComponent<Animator>()?.SetTrigger("Spawn");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        controller.StartExplosionTimer();
    }

    public void NotifySpawnFinished()
    {
        finished = true;
    }

    public override void Execute()
    {
        if (finished)
        {
            // El controller se encarga de llamar a fsm.Transition
            controller.Transition(EnemyInputs.LostPlayer);
        }
    }

    public override void Sleep()
    {
        base.Sleep();
        controller.RegisterSpawnState(null);
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
}