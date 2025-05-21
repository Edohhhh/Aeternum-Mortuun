using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour, IEnemyDataProvider
{
    public Transform player;
    public float detectionRadius = 3f;
    public float attackDistance = 1f;
    public float maxHealth = 100f;
    public float damage = 10f;
    public float maxSpeed = 3f;
    public float acceleration = 10f;

    private FSM<EnemyInputs> fsm;
    private HealthSystem health;

    public GameObject miniSlimePrefab;

    private void Start()
    {
        health = GetComponent<HealthSystem>();

        EnemyIdleState Enemyidle = new EnemyIdleState(transform);
        EnemyAttackState Enemyattack = new EnemyAttackState(transform, player);
        SlimeDeathState Enemydeath = new SlimeDeathState(this);

        Enemyidle.AddTransition(EnemyInputs.SeePlayer, Enemyattack);
        Enemyattack.AddTransition(EnemyInputs.LostPlayer, Enemyidle);

        Enemyattack.AddTransition(EnemyInputs.Die, Enemydeath);
        Enemyidle.AddTransition(EnemyInputs.Die, Enemydeath);

        fsm = new FSM<EnemyInputs>(Enemyidle);
    }

    private void Update()
    {
        fsm.Update();

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= detectionRadius)
            Transition(EnemyInputs.SeePlayer);
        else
            Transition(EnemyInputs.LostPlayer);
    }

    public void Transition(EnemyInputs input)
    {
        fsm.Transition(input);
    }

    public void Die()
    {
        Instantiate(miniSlimePrefab, transform.position + Vector3.right * 1.5f, Quaternion.identity);
        Instantiate(miniSlimePrefab, transform.position + Vector3.left * 1.5f, Quaternion.identity);
        Destroy(gameObject);
    }

    public float GetCurrentHealth()
    {
        return health.GetCurrentHealth();
    }

    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => attackDistance;
    public float GetDamage() => damage;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;
}
