using UnityEngine;
using static SkeletonController;

public class SpawnMinionState : State<EnemyInputs>
{
    private readonly SkeletonController controller;
    private readonly MinionSpawnEntry[] entries;   // ← NUEVO
    private readonly float duration;

    private float timer = 0f;
    private bool invoked = false;

    private float originalGravity;
    private RigidbodyConstraints2D originalConstraints;

    public SpawnMinionState(
        SkeletonController controller,
        MinionSpawnEntry[] entries,   // ← NUEVO
        float duration
    )
    {
        this.controller = controller;
        this.entries = entries;       // ← NUEVO
        this.duration = duration;
    }

    public override void Awake()
    {
        base.Awake();

        // 1) Bloquea físicas igual que antes
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            originalGravity = rb.gravityScale;
            originalConstraints = rb.constraints;

            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.constraints |= RigidbodyConstraints2D.FreezePosition;
        }

        timer = 0f;
        invoked = false;

        // 2) Disparar trigger de animación
        controller.GetComponent<Animator>()?.SetTrigger("SpawnMinions");
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        // Si pasa la duración y aun no invocó
        if (timer >= duration && !invoked)
        {
            invoked = true;
            Debug.Log("[SpawnMinionState] Timer completo: instanciando minions");

            // ---------------------------
            //   MULTI-PREFAB + MULTI-POINT
            // ---------------------------
            foreach (var e in entries)
            {
                if (e.prefab == null || e.spawnPoint == null)
                    continue;

                Object.Instantiate(
                    e.prefab,
                    e.spawnPoint.position,
                    e.spawnPoint.rotation
                );
            }

            // Vuelve al estado Follow
            controller.Transition(EnemyInputs.SeePlayer);

            // Programa UnderGroundAttack luego del delay
            controller.Invoke(
                nameof(SkeletonController.DoUnderGroundAttack),
                controller.postSpawnUnderGroundDelay
            );
        }
    }

    public override void Sleep()
    {
        base.Sleep();

        // Restaurar físicas
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = originalGravity;
            rb.constraints = originalConstraints;
        }
    }
}
