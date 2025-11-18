using UnityEngine;
using System.Collections;

public class ArqueroController : MonoBehaviour, IEnemyDataProvider
{
    [Header("References")]
    [SerializeField] private Transform player; // Mantenemos esto [SerializeField]
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth health;

    [Header("Stats (de PDF)")]
    [SerializeField] private float detectionRadius = 10f;

    [Header("Shooting (de PDF)")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float shootCooldown = 5f;
    [SerializeField] public float projectileSpeed = 20f;
    [SerializeField] public float projectileLifetime = 10f;

    private float lastShootTime = -99f;

    private FSM<EnemyInputs> fsm;
    private ArqueroSpawnState _spawnStateRef;

    public void RegisterSpawnState(ArqueroSpawnState s) => _spawnStateRef = s;
    public void OnSpawnFinished() => _spawnStateRef?.NotifySpawnFinished();

    public bool IsSpawning() => fsm.GetCurrentState() is ArqueroSpawnState;
    public bool IsShooting() => fsm.GetCurrentState() is ArqueroShootState;


    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();

        // --- AÑADIDO: ENCONTRAR AL JUGADOR AUTOMÁTICAMENTE ---
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("ArqueroController: No se pudo encontrar un objeto con el tag 'Player'.");
            }
        }
        // --- FIN DE LA ADICIÓN ---

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        SetupFSM();
    }

    private void SetupFSM()
    {
        fsm = new FSM<EnemyInputs>();

        var spawnState = new ArqueroSpawnState(this);
        var idleState = new ArqueroIdleState(this, animator, rb);
        var shootState = new ArqueroShootState(this, animator);
        var deathState = new ArqueroDeathState(this, animator, rb);

        spawnState.SetFSM(fsm);
        idleState.SetFSM(fsm);
        shootState.SetFSM(fsm);
        deathState.SetFSM(fsm);

        spawnState.AddTransition(EnemyInputs.LostPlayer, idleState);
        idleState.AddTransition(EnemyInputs.MeleeAttack, shootState);
        shootState.AddTransition(EnemyInputs.LostPlayer, idleState);

        spawnState.AddTransition(EnemyInputs.Die, deathState);
        idleState.AddTransition(EnemyInputs.Die, deathState);
        shootState.AddTransition(EnemyInputs.Die, deathState);

        fsm.SetInit(spawnState);
    }

    void Update()
    {
        fsm.Update();

        if (fsm.GetCurrentState() is ArqueroIdleState || fsm.GetCurrentState() is ArqueroShootState)
        {
            if (player != null && spriteRenderer != null)
            {
                spriteRenderer.flipX = (player.position.x < transform.position.x);
            }
        }
    }

    // --- Métodos de Disparo ---
    public bool CanShoot()
    {
        return Time.time >= lastShootTime + shootCooldown;
    }
    public void MarkShootAsUsed()
    {
        lastShootTime = Time.time;
    }
    public void FireProjectile()
    {
        if (projectilePrefab == null || shootPoint == null || player == null) return;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (player.position.x < transform.position.x);
        }

        Vector2 direction = (player.position - shootPoint.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

        ProyectilEnemigo scriptProyectil = projectile.GetComponent<ProyectilEnemigo>();
        if (scriptProyectil != null)
        {
            scriptProyectil.Initialize(direction, projectileSpeed, projectileLifetime);
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // --- Interfaz IEnemyDataProvider ---
    public void Transition(EnemyInputs input) => fsm.Transition(input);
    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetCurrentHealth() => health != null ? health.GetCurrentHealth() : 999f;
    public bool IsPlayerInDetectionRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }
    public float GetMaxSpeed() => 0f;
    public float GetAcceleration() => 0f;
    public float GetAttackDistance() => 0f;
    public float GetDamage() => 0f;
}