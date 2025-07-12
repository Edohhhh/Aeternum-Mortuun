using UnityEngine;

public class SpawnMinionState : State<EnemyInputs>
{
    private readonly SkeletonController controller;
    private readonly GameObject prefab;
    private readonly Transform[] spawnPoints;
    private readonly float duration;   // Duraci�n de la anim
    private float timer = 0f;
    private bool invoked = false;

    private float originalGravity;
    private RigidbodyConstraints2D originalConstraints;

    public SpawnMinionState(
        SkeletonController controller,
        GameObject prefab,
        Transform[] spawnPoints,
        float duration          // lo recibimos
    )
    {
        this.controller = controller;
        this.prefab = prefab;
        this.spawnPoints = spawnPoints;
        this.duration = duration;
    }

    public override void Awake()
    {
        base.Awake();
        // 1) Bloquea la f�sica para que no caiga ni se desplace
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // guarda estado original
            originalGravity = rb.gravityScale;
            originalConstraints = rb.constraints;

            // anula gravedad y congela posici�n
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
            rb.constraints |= RigidbodyConstraints2D.FreezePosition;
        }
        timer = 0f;
        invoked = false;
        // Disparamos la anim de invocaci�n
        controller.GetComponent<Animator>()?.SetTrigger("SpawnMinions");
    }

    public override void Execute()
    {
        // 1) Vamos acumulando tiempo
        //Debug.Log($"[SpawnMinionState] timer={timer:F2}, invoked={invoked}");
        timer += Time.deltaTime;

        // 2) Cuando pasa la duraci�n de la anim Y a�n no hemos invocado:
        if (timer >= duration && !invoked)
        {
            invoked = true;
            Debug.Log("[SpawnMinionState] Timer completo: instanciando minions");

            // Instanciamos
            foreach (var pt in spawnPoints)
                Object.Instantiate(prefab, pt.position, pt.rotation);

            // Volvemos a Follow
            controller.Transition(EnemyInputs.SeePlayer);

            // Programamos el UnderGroundAttack tras un peque�o delay
            controller.Invoke(
                nameof(SkeletonController.DoUnderGroundAttack),
                controller.postSpawnUnderGroundDelay
            );
        }
    }

    public override void Sleep()
    {
        base.Sleep();

        // 2) Restaura la f�sica justo al salir del estado
        var rb = controller.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = originalGravity;
            rb.constraints = originalConstraints;
        }
    }
}
