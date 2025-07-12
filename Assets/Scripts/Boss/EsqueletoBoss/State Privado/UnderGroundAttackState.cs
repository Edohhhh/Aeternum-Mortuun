using UnityEngine;

public class UnderGroundAttackState : State<EnemyInputs>
{
    private enum Phase { Burying, Emerging }
    private Phase phase;
    private readonly float buryDuration;
    private readonly float emergeDuration;
    private float timer;

    private readonly SkeletonController controller;
    private float originalGravity;
    private RigidbodyConstraints2D originalConstraints;

    public UnderGroundAttackState(
        SkeletonController controller,
        float buryDuration,
        float emergeDuration
    )
    {
        this.controller = controller;
        this.buryDuration = buryDuration;
        this.emergeDuration = emergeDuration;
    }

    public override void Awake()
    {
        base.Awake();
        phase = Phase.Burying;
        timer = 0f;

        // 1) Lanzamos anim de enterrarse
        controller.GetComponent<Animator>()?.SetTrigger("Burrow");
        // NO congelamos nada: durante Burying se debe mover
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Burying:
                // Mientras "Burrow" (timer<buryDuration), deslízate
                Vector3 current = controller.transform.position;
                Vector3 target = controller.GetPlayer().position;
                float speed = controller.GetMaxSpeed();

                controller.transform.position =
                    Vector3.MoveTowards(current, target, speed * Time.deltaTime);

                // Al acabar buryDuration, pasamos a fase Emerging
                if (timer >= buryDuration)
                {
                    phase = Phase.Emerging;
                    timer = 0f;

                    // 2) Congelamos física y saltamos a "Spawn" (emerge)
                    var rb = controller.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        originalGravity = rb.gravityScale;
                        originalConstraints = rb.constraints;

                        rb.gravityScale = 0f;
                        rb.linearVelocity = Vector2.zero;
                        rb.constraints |= RigidbodyConstraints2D.FreezePosition;
                    }

                    // Reutilizamos el clip "Spawn" para emergir
                    controller.GetComponent<Animator>()?.SetTrigger("Spawn");
                }
                break;

            case Phase.Emerging:
                // Mantente quieto hasta que termine emergeDuration
                if (timer >= emergeDuration)
                {
                    // 3) Restaurar física y volver a Follow
                    var rb = controller.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.gravityScale = originalGravity;
                        rb.constraints = originalConstraints;
                    }
                    controller.Transition(EnemyInputs.SeePlayer);
                }
                break;
        }
    }

    public override void Sleep()
    {
        base.Sleep();
        // Aseguramos restaurar física si nos movieron
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = originalGravity;
            rb.constraints = originalConstraints;
        }
    }
}