using UnityEngine;

public class MiniSlimeController : MonoBehaviour, IEnemyDataProvider
{
    public Transform player;
    public float detectionRadius = 2f;
    public float attackDistance = 0.8f;
    public float maxHealth = 50f;
    public float damage = 10f;
    public float maxSpeed = 2f;
    public float acceleration = 4f;
    public GameObject miniSlimePrefab;

    private FSM<EnemyInputs> fsm;
    private HealthSystem health;

    private void Start()
    {
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

        health = GetComponent<HealthSystem>();

        EnemyIdleState idle = new EnemyIdleState(transform);
        EnemyAttackState attack = new EnemyAttackState(transform, player);
        MiniSlimeDeathState death = new MiniSlimeDeathState(this);

        idle.AddTransition(EnemyInputs.SeePlayer, attack);
        attack.AddTransition(EnemyInputs.LostPlayer, idle);

        idle.AddTransition(EnemyInputs.Die, death);
        attack.AddTransition(EnemyInputs.Die, death);

        fsm = new FSM<EnemyInputs>(idle);

        
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
        Vector3 pos = transform.position;

        Instantiate(miniSlimePrefab, pos + new Vector3(1.5f, 1.5f, 0), Quaternion.identity);
        Instantiate(miniSlimePrefab, pos + new Vector3(-1.5f, 1.5f, 0), Quaternion.identity);
        Instantiate(miniSlimePrefab, pos + new Vector3(1.5f, -1.5f, 0), Quaternion.identity);
        Instantiate(miniSlimePrefab, pos + new Vector3(-1.5f, -1.5f, 0), Quaternion.identity);

        Destroy(gameObject);
    }

    public float GetCurrentHealth() => health.GetCurrentHealth();
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => attackDistance;
    public float GetDamage() => damage;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;
}


