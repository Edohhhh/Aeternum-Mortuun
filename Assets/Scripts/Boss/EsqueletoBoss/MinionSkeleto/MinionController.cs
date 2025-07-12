using UnityEngine;
using System.Collections;

public class MinionController : MonoBehaviour, IEnemyDataProvider
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Stats")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float maxSpeed = 2.5f;

    [Header("Spawn")]
    [Tooltip("Duración en segundos de la anim de aparición")]
    [SerializeField] private float spawnAnimDuration = 1f;

    private FSM<EnemyInputs> fsm;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth health;
    private SpriteRenderer sprite;
    private Rigidbody2D rb;

    // Helper para “¿estoy en Spawn?”    
    public bool IsSpawning() => fsm.GetCurrentState() is MinionSpawnState;
    private void Start()
    {

        EnemyManager.Instance.RegisterEnemy();

        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("[MinionController] No se encontró ningún GameObject con tag 'Player'.");
        }

        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();

        if (health != null)
            health.OnDeath += () => Transition(EnemyInputs.Die);

        // Asegúrate de no rotar nunca
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

        // FSM
        var spawn = new MinionSpawnState(this, spawnAnimDuration);
        var idle = new EnemyIdleState(transform);
        var follow = new EnemyFollowState(transform, player, maxSpeed);
        var death = new EnemyDeathState(this);

        fsm = new FSM<EnemyInputs>(spawn);

        spawn.AddTransition(EnemyInputs.SeePlayer, follow);
        spawn.AddTransition(EnemyInputs.Die, death);

        idle.AddTransition(EnemyInputs.SeePlayer, follow);
        idle.AddTransition(EnemyInputs.Die, death);

        follow.AddTransition(EnemyInputs.LostPlayer, idle);
        follow.AddTransition(EnemyInputs.Die, death);
    }

    private void Update()
    {
        fsm.Update();

        // Durante spawn, no decido nada más
        if (IsSpawning())
            return;

        // Transiciones automáticas See/Lost
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= detectionRadius) Transition(EnemyInputs.SeePlayer);
        else Transition(EnemyInputs.LostPlayer);

        // Animación de caminar
        bool walking = fsm.GetCurrentState() is EnemyFollowState;
        animator.SetBool("isWalking", walking);

        // Flip X
        if (player != null)
            sprite.flipX = (player.position.x - transform.position.x) < 0;
    }

    public void Transition(EnemyInputs input) => fsm.Transition(input);

   
    public IEnumerator DelayedDeath()
    {
        EnemyManager.Instance.UnregisterEnemy();
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
    public float GetCurrentHealth() => health != null ? health.GetCurrentHealth() : 0f;
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetAttackDistance() => detectionRadius;
    public float GetDamage() => 0f;
    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => 0f;
}
