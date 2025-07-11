using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour, IEnemyDataProvider
{
    [SerializeField] private GameObject acidPrefab; // en el controller
    [SerializeField] private float specialCooldown = 10f;
    public Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public float detectionRadius = 3f;
    public float attackDistance = 1f;
    public float damage = 10f;
    public float maxSpeed = 3f;
    public float acceleration = 10f;
    private float lastSpecialTime;
    private float nextSpecialTime;

    //private bool alreadyUnregistered = false;

    private FSM<EnemyInputs> fsm;
    private EnemyHealth health;

    public GameObject miniSlimePrefab;

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += () => Transition(EnemyInputs.Die);
        }

        EnemyIdleState Enemyidle = new EnemyIdleState(transform);
        EnemyAttackState Enemyattack = new EnemyAttackState(transform, acidPrefab);
        SlimeDeathState Enemydeath = new SlimeDeathState(this);

        fsm = new FSM<EnemyInputs>(Enemyidle);

        EspecialAtackSlime especial = new EspecialAtackSlime(this, fsm);
        ChargeSlimeState charge = new ChargeSlimeState(this, fsm, acidPrefab);

        Enemyidle.AddTransition(EnemyInputs.SeePlayer, Enemyattack);
        Enemyattack.AddTransition(EnemyInputs.LostPlayer, Enemyidle);

        Enemyattack.AddTransition(EnemyInputs.Die, Enemydeath);
        Enemyidle.AddTransition(EnemyInputs.Die, Enemydeath);

        Enemyattack.AddTransition(EnemyInputs.SpecialAttack, especial);
        especial.AddTransition(EnemyInputs.SeePlayer, Enemyattack);
        especial.AddTransition(EnemyInputs.LostPlayer, Enemyidle);
        especial.AddTransition(EnemyInputs.Die, Enemydeath);

        especial.AddTransition(EnemyInputs.SpecialAttack, charge); // viene del azul
        charge.AddTransition(EnemyInputs.SeePlayer, Enemyattack); // vuelve al normal
        charge.AddTransition(EnemyInputs.LostPlayer, Enemyidle);
        charge.AddTransition(EnemyInputs.Die, Enemydeath);


    }

    private void Update()
    {
        fsm.Update();

        if (IsBusyWithSpecial()) return;

        //if (fsm.GetCurrentState() is EspecialAtackSlime)
        //    return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= detectionRadius)
            Transition(EnemyInputs.SeePlayer);
        else
            Transition(EnemyInputs.LostPlayer);

        animator.SetBool("isWalking", fsm.GetCurrentState() is EnemyAttackState);

        if (fsm.GetCurrentState() is EnemyAttackState && player != null)
        {
            Vector2 direction = player.position - transform.position;
            spriteRenderer.flipX = direction.x < 0;
        }

        //float distance = Vector2.Distance(transform.position, player.position);
        //if (distance <= detectionRadius)
        //    Transition(EnemyInputs.SeePlayer);
        //else
        //    Transition(EnemyInputs.LostPlayer);

        //animator.SetBool("isWalking", fsm.GetCurrentState() is EnemyAttackState);

        //if (fsm.GetCurrentState() is EnemyAttackState && player != null)
        //{
        //    Vector2 direction = player.position - transform.position;
        //    spriteRenderer.flipX = direction.x < 0;
        //}

        //// <<< ATAQUE ESPECIAL AUTOMÁTICO >>>
        //if (Time.time >= nextSpecialTime)
        //{
        //    Debug.Log("Auto: Activando ataque especial por tiempo");
        //    nextSpecialTime = Time.time + specialCooldown; // Reinicia cooldown
        //    Transition(EnemyInputs.SpecialAttack);
        //}
    }

    public void Transition(EnemyInputs input)
    {
        fsm.Transition(input);
    }

    private IEnumerator DelayedDeath()
    {
        yield return new WaitForEndOfFrame();
        EnemyManager.Instance.UnregisterEnemy();
        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return health != null ? health.GetCurrentHealth() : 0f;
    }

    public bool CanUseSpecialAttack()
    {
        return Time.time >= lastSpecialTime + specialCooldown;
    }

    public void MarkSpecialUsed()
    {
        lastSpecialTime = Time.time;
    }

    public bool IsInSpecialAttackState()
    {
        return fsm.GetCurrentState() is EspecialAtackSlime;
    }

    public bool IsInChargeState()
    {
        return fsm.GetCurrentState() is ChargeSlimeState;
    }

    public bool IsBusyWithSpecial()
    {
        return fsm.GetCurrentState() is EspecialAtackSlime || fsm.GetCurrentState() is ChargeSlimeState;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (fsm.GetCurrentState() is ICollisionHandler handler)
        {
            handler.OnCollisionEnter2D(collision);
        }
    }

    public GameObject GetMiniSlimePrefab() => miniSlimePrefab;
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => attackDistance;
    public float GetDamage() => damage;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;
}
