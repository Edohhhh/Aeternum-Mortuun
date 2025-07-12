using UnityEngine;

public class MinionSpawnState : State<EnemyInputs>
{
    readonly MinionController controller;
    readonly float duration;
    float timer;
    bool transitioned;

    public MinionSpawnState(MinionController ctrl, float spawnAnimDuration)
    {
        controller = ctrl;
        duration = spawnAnimDuration;
    }

    public override void Awake()
    {
        base.Awake();
        timer = 0f;
        transitioned = false;

        // 1) Disparar anim de spawn
        controller.GetComponent<Animator>()?.SetTrigger("Spawn");

        // 2) Congelar posición para que no lo empuje ni se mueva
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.constraints |= RigidbodyConstraints2D.FreezePosition;
    }

    public override void Execute()
    {
        if (transitioned) return;

        timer += Time.deltaTime;
        if (timer < duration) return;

        transitioned = true;

        // 3) Liberar la posición (seguimos congelando Z)
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints &= ~RigidbodyConstraints2D.FreezePosition;
            rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        // 4) Pasar a seguir al jugador
        controller.Transition(EnemyInputs.SeePlayer);
    }

    public override void Sleep()
    {
        base.Sleep();
        // no hay nada más que limpiar
    }
}