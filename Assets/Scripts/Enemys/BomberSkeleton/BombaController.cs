using UnityEngine;
using System.Collections;

// NO volvemos a declarar 'EnemyInputs' aquí, ya existe en otro script.

public class BombaController : MonoBehaviour, IEnemyDataProvider
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject explosionVFX;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth health; // Asigna tu script de vida

    [Header("Stats (de PDF)")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float maxSpeed = 3f; // 20 era mucho, ajusta esto
    [SerializeField] private float acceleration = 5f; // 30 era mucho, ajusta esto

    [Header("Bomba Params (de PDF)")]
    [SerializeField] public float explosionTime = 5f;
    [SerializeField] public float deathExplosionDelay = 2f;

    [Header("Blink Effect")]
    [SerializeField] private Color blinkColor = Color.white;
    [SerializeField] private float blinkBaseFrequency = 0.5f;
    [SerializeField] private float blinkEndFrequency = 0.1f;

    // --- Variables Internas ---
    private FSM<EnemyInputs> fsm;
    private float spawnTimestamp;
    private bool isTimerRunning = false;

    // --- Hooks para Estados ---
    private BombaSpawnState _spawnStateRef;
    public void RegisterSpawnState(BombaSpawnState s) => _spawnStateRef = s;
    public void OnSpawnFinished() => _spawnStateRef?.NotifySpawnFinished(); // Llamar desde Animation Event

    // --- Flags de Estado ---
    public bool IsSpawning() => fsm.GetCurrentState() is BombaSpawnState;
    public bool IsExploding() => fsm.GetCurrentState() is BombaExplosionState || fsm.GetCurrentState() is BombaDeathExplosionState;


    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>(); // Ajusta esto a tu script de vida

        SetupFSM();
    }

    private void SetupFSM()
    {
        fsm = new FSM<EnemyInputs>();

        // 1. Crear instancias de los estados
        var spawnState = new BombaSpawnState(this);

        // --- USA LOS NUEVOS SCRIPTS ---
        var idleState = new BombaIdleState(this, animator, rb);
        var followState = new BombaFollowState(this, animator, rb, this.transform);
        // -------------------------

        var explodeState = new BombaExplosionState(this, animator, rb);
        var deathState = new BombaDeathExplosionState(this, animator, rb);

        // 2. Asignar la FSM a cada estado (usa el método 'SetFSM' de tu clase 'State')
        spawnState.SetFSM(fsm);
        idleState.SetFSM(fsm);
        followState.SetFSM(fsm);
        explodeState.SetFSM(fsm);
        deathState.SetFSM(fsm);

        // 3. Definir transiciones DENTRO de cada estado
        // (Adaptado a tu FSM descentralizada)

        // Transiciones de SpawnState
        spawnState.AddTransition(EnemyInputs.LostPlayer, idleState);

        // Transiciones de IdleState
        idleState.AddTransition(EnemyInputs.SeePlayer, followState);

        // Transiciones de FollowState
        followState.AddTransition(EnemyInputs.LostPlayer, idleState);

        // 4. Transiciones "Globales" (Forzadas por el Árbol de Decisión)
        spawnState.AddTransition(EnemyInputs.Die, deathState);
        idleState.AddTransition(EnemyInputs.Die, deathState);
        followState.AddTransition(EnemyInputs.Die, deathState);

        spawnState.AddTransition(EnemyInputs.Explode, explodeState);
        idleState.AddTransition(EnemyInputs.Explode, explodeState);
        followState.AddTransition(EnemyInputs.Explode, explodeState);

        // 5. Estado inicial (Usando tu método `SetInit`)
        fsm.SetInit(spawnState);
    }

    void Update()
    {
        fsm.Update(); // Llama a Execute() del estado actual

        // Lógica de Flip (SpriteRenderer)
        // Apunta al nuevo script 'BombaFollowState'
        if (fsm.GetCurrentState() is BombaFollowState && player != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = (player.position.x < transform.position.x);
        }
    }

    void FixedUpdate()
    {
        // Llama a fsm.FixedUpdate(), que llama a _currentState.FixedExecute()
        fsm?.FixedUpdate();
    }

    // --- Lógica del Temporizador ---

    public void StartExplosionTimer()
    {
        spawnTimestamp = Time.time;
        isTimerRunning = true;
        StartCoroutine(BlinkCoroutine());
    }

    public bool IsExplosionTimerDone()
    {
        if (!isTimerRunning) return false;
        return (Time.time - spawnTimestamp) >= explosionTime;
    }

    public void StopExplosionTimer()
    {
        isTimerRunning = false;
    }

    private IEnumerator BlinkCoroutine()
    {
        Color originalColor = spriteRenderer.color;
        while (isTimerRunning)
        {
            float elapsed = Time.time - spawnTimestamp;
            float progress = elapsed / explosionTime;
            float currentFrequency = Mathf.Lerp(blinkBaseFrequency, blinkEndFrequency, progress);

            spriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(currentFrequency * 0.3f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(currentFrequency * 0.7f);
        }
        spriteRenderer.color = originalColor;
    }

    // --- Lógica de Explosión ---

    public void PerformExplosion()
    {
        StopExplosionTimer();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // (Aquí va tu lógica de daño en área)

        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // --- Interfaz IEnemyDataProvider (Implementación) ---

    // Este método es llamado por los estados (ej. BombaSpawnState)
    public void Transition(EnemyInputs input) => fsm.Transition(input);

    public Transform GetPlayer() => player;
    public float GetDetectionRadius() => detectionRadius;
    public float GetCurrentHealth() => health != null ? health.GetCurrentHealth() : 999f;

    public float GetMaxSpeed() => maxSpeed;
    public float GetAcceleration() => acceleration;

    public bool IsPlayerInDetectionRange()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    // Métodos que no usamos aquí, pero la interfaz requiere
    public float GetAttackDistance() => 0f;
    public float GetDamage() => 0f;
}