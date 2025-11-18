using UnityEngine;

public class ArqueroSpawnState : State<EnemyInputs>
{
    private readonly ArqueroController controller;
    private Rigidbody2D rb;
    private bool finished;

    public ArqueroSpawnState(ArqueroController ctrl)
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

        // El Arquero es Kinematic, pero por si acaso
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void NotifySpawnFinished()
    {
        finished = true;
    }

    public override void Execute()
    {
        if (finished)
        {
            // Transicionamos a Idle (el �rbol de Decisi�n tomar� el control)
            controller.Transition(EnemyInputs.LostPlayer);
        }
    }

    public override void Sleep()
    {
        base.Sleep();
        controller.RegisterSpawnState(null);
    }
}