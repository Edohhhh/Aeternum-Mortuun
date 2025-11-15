using UnityEngine;
using System.Collections;

public class BombaDeathExplosionState : State<EnemyInputs>
{
    private readonly BombaController controller;
    private readonly Animator animator;
    private readonly Rigidbody2D rb;

    public BombaDeathExplosionState(BombaController ctrl, Animator anim, Rigidbody2D rigidBody)
    {
        controller = ctrl;
        animator = anim;
        rb = rigidBody;
    }

    public override void Awake()
    {
        base.Awake();
        controller.StopExplosionTimer();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetTrigger("Die");

        controller.StartCoroutine(DelayedExplosion());
    }

    private IEnumerator DelayedExplosion()
    {
        yield return new WaitForSeconds(controller.deathExplosionDelay);
        controller.PerformExplosion();
    }
}