using UnityEngine;

public class EsqueletoSpawnState : State<EnemyInputs>
{
    private readonly SkeletonController controller;
    private readonly float duration;
    private float timer;
    private bool hasTransitioned;

    public EsqueletoSpawnState(SkeletonController ctrl, float spawnAnimDuration)
    {
        controller = ctrl;
        duration = spawnAnimDuration;
    }

    public override void Awake()
    {
        base.Awake();
        timer = 0f;
        hasTransitioned = false;
        controller.GetComponent<Animator>()?.SetTrigger("Spawn");
    }

    public override void Execute()
    {
        if (hasTransitioned) return;

        timer += Time.deltaTime;
        if (timer < duration) return;

        hasTransitioned = true;
        controller.Transition(EnemyInputs.SeePlayer);
        //  SOLO invocamos minions tras un pequeño paseo
        controller.Invoke(
            nameof(SkeletonController.DoSpawnMinions),
            controller.initialSpawnMinionDelay
            );
    }
}
