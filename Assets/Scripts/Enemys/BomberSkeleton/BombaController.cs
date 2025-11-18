using UnityEngine;
using System.Collections;

public class BombaController : MonoBehaviour, IEnemyDataProvider
{
    [Header("References")]
    [SerializeField] private Transform player; // Mantenemos esto [SerializeField]
    [SerializeField] private GameObject explosionVFX;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private EnemyHealth health;

    [Header("Stats (de PDF)")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float minStandoffDistance = 1.5f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float acceleration = 5f;

    [Header("Bomba Params (de PDF)")]
    [SerializeField] public float explosionTime = 5f;
    [SerializeField] public float deathExplosionDelay = 2f;

    [Header("Blink Effect")]
    [SerializeField] private Color blinkColor = Color.white;
    [SerializeField] private float blinkBaseFrequency = 0.5f;
    [SerializeField] private float blinkEndFrequency = 0.1f;

    private FSM<EnemyInputs> fsm;
    private float spawnTimestamp;
    private bool isTimerRunning = false;
    private bool isPlayerInStandoffRange = false;

    private BombaSpawnState _spawnStateRef;
    public void RegisterSpawnState(BombaSpawnState s) => _spawnStateRef = s;
    public void OnSpawnFinished() => _spawnStateRef?.NotifySpawnFinished();

    public bool IsSpawning() => fsm.GetCurrentState() is BombaSpawnState;
    public bool IsExploding() => fsm.GetCurrentState() is BombaExplosionState || fsm.GetCurrentState() is BombaDeathExplosionState;


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
                Debug.LogWarning("BombaController: No se pudo encontrar un objeto con el tag 'Player'.");
            }
        }
        // --- FIN DE LA ADICIÓN ---

        SetupFSM();
    }

    private void SetupFSM()
    {
        fsm = new FSM<EnemyInputs>();

        var spawnState = new BombaSpawnState(this);
        var idleState = new BombaIdleState(this, animator, rb);
        var followState = new BombaFollowState(this, animator, rb, this.transform);
        var explodeState = new BombaExplosionState(this, animator, rb);
        var deathState = new BombaDeathExplosionState(this, animator, rb);

        // Asignar FSM
        spawnState.SetFSM(fsm);
        idleState.SetFSM(fsm);
        followState.SetFSM(fsm);
        explodeState.SetFSM(fsm);
        deathState.SetFSM(fsm);

        // Transiciones
        spawnState.AddTransition(EnemyInputs.LostPlayer, idleState);
        idleState.AddTransition(EnemyInputs.SeePlayer, followState);
        followState.AddTransition(EnemyInputs.LostPlayer, idleState);

        // Transiciones Globales (para Die y Explode)
        spawnState.AddTransition(EnemyInputs.Die, deathState);
        idleState.AddTransition(EnemyInputs.Die, deathState);
        followState.AddTransition(EnemyInputs.Die, deathState);
        spawnState.AddTransition(EnemyInputs.Explode, explodeState);
        idleState.AddTransition(EnemyInputs.Explode, explodeState);
        followState.AddTransition(EnemyInputs.Explode, explodeState);

        fsm.SetInit(spawnState);
    }

    void Update()
    {
        fsm.Update();

        if (fsm.GetCurrentState() is BombaFollowState && player != null && spriteRenderer != null)
        {
            spriteRenderer.flipX = (player.position.x < transform.position.x);
        }
    }

    void FixedUpdate()
    {
        fsm?.FixedUpdate();
    }

    // --- Triggers para la distancia mínima ---
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInStandoffRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInStandoffRange = false;
        }
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
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    // --- Función para el Árbol de Decisión ---
    public bool IsPlayerInStandoffRange()
    {
        return isPlayerInStandoffRange;
    }

    // --- Interfaz IEnemyDataProvider ---
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
    public float GetAttackDistance() => 0f;
    public float GetDamage() => 0f;
}