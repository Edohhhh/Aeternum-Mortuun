using System.Collections;
using UnityEngine;

public class MiniMiniSlimeController : MonoBehaviour, IEnemyDataProvider
{
    public Transform player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public float detectionRadius = 2f;
    public float attackDistance = 0.8f;
    public float maxHealth = 20f;
    public float damage = 5f;
    public float maxSpeed = 2f;
    public float acceleration = 3f;

    private FSM<EnemyInputs> fsm;
    private HealthSystem health;

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("MiniSlime no encontró al jugador con el tag 'Player'");
            }
        }

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<HealthSystem>();

        EnemyIdleState idle = new EnemyIdleState(transform);
        EnemyAttackState attack = new EnemyAttackState(transform);
        SlimeDeathStateSimple death = new SlimeDeathStateSimple(this);

        idle.AddTransition(EnemyInputs.SeePlayer, attack);
        attack.AddTransition(EnemyInputs.LostPlayer, idle);

        idle.AddTransition(EnemyInputs.Die, death);
        attack.AddTransition(EnemyInputs.Die, death);

        fsm = new FSM<EnemyInputs>(idle);

        health.OnDeath += HandleDeath;
    }

    private void Update()
    {
        fsm.Update();

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
    }

    public void Transition(EnemyInputs input)
    {
        fsm.Transition(input);
    }

    public void Die()
    {
        EnemyManager.Instance.UnregisterEnemy();
        Destroy(gameObject);

    }

    private void HandleDeath()
    {
        Transition(EnemyInputs.Die); 
    }

    public float GetCurrentHealth() => health.GetCurrentHealth();
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => attackDistance;
    public float GetDamage() => damage;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;
}
