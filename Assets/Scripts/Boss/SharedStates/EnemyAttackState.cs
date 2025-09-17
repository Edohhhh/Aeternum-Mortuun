using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackState : State<EnemyInputs>
{
    private Transform enemy;
    private IEnemyDataProvider data;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector2 currentVelocity = Vector2.zero;

    private float acidSpawnInterval = 0.2f;
    private float acidSpawnTimer = 0f;
    private GameObject acidPrefab;

    public EnemyAttackState(Transform enemy, GameObject acidPrefab)
    {
        this.enemy = enemy;
        this.data = enemy.GetComponent<IEnemyDataProvider>();
        this.rb = enemy.GetComponent<Rigidbody2D>();
        this.spriteRenderer = enemy.GetComponent<SpriteRenderer>();
        this.acidPrefab = acidPrefab;
    }

    public override void Awake()
    {
        base.Awake();
        if (rb != null)
        {
            // Opcional, pero ayuda a que se vea suave y consistente
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    // LÓGICA NO FÍSICA (spawns/timers/IA)
    public override void Execute()
    {
        if (data == null || data.GetPlayer() == null) return;

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null && health.IsStunned) return;

        // Spawnear ácido a intervalo fijo (independiente del framerate)
        acidSpawnTimer += Time.deltaTime;
        if (acidSpawnTimer >= acidSpawnInterval)
        {
            acidSpawnTimer = 0f;
            GameObject.Instantiate(acidPrefab, enemy.position, Quaternion.identity);
        }
    }

    // SOLO FÍSICA
    public override void FixedExecute()
    {
        if (data == null || data.GetPlayer() == null) return;

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null && health.IsStunned) return;

        // Dirección hacia el jugador
        Vector2 toPlayer = (data.GetPlayer().position - enemy.position);
        Vector2 direction = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;

        // Velocidad deseada
        float speed = data.GetMaxSpeed();
        currentVelocity = direction * speed;

        // Movimiento consistente: usar fixedDeltaTime
        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);

        // (Opcional) recortar picos si algo mete velocidad extra
        if (rb.linearVelocity.magnitude > speed * 1.1f)
            rb.linearVelocity = rb.linearVelocity.normalized * speed;

        // Mantener rotación nula (flipX lo maneja el controller)
        enemy.rotation = Quaternion.identity;
    }
}
//public class EnemyAttackState : State<EnemyInputs>
//{
//    private Transform enemy;
//    private IEnemyDataProvider data;
//    private SpriteRenderer spriteRenderer;
//    private Rigidbody2D rb;
//    private Vector2 currentVelocity = Vector2.zero;

//    private float acidSpawnInterval = 0.2f; // tiempo entre gotas de ácido
//    private float acidSpawnTimer = 0f;
//    private GameObject acidPrefab;

//    public EnemyAttackState(Transform enemy, GameObject acidPrefab)
//    {
//        this.enemy = enemy;
//        this.data = enemy.GetComponent<IEnemyDataProvider>();
//        this.rb = enemy.GetComponent<Rigidbody2D>();
//        this.spriteRenderer = enemy.GetComponent<SpriteRenderer>();
//        this.acidPrefab = acidPrefab; // lo recibís desde el slime
//    }

//    public override void Awake()
//    {
//        base.Awake(); 
//    }

//    public override void Execute()
//    {
//        if (data == null || data.GetPlayer() == null) return;

//        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
//        if (health != null && health.IsStunned) return;

//        //  Dirección normalizada hacia el jugador
//        Vector2 direction = (data.GetPlayer().position - enemy.position).normalized;

//        //  Movimiento consistente en todas las máquinas
//        float speed = data.GetMaxSpeed();
//        currentVelocity = direction * speed;

//        // rb.MovePosition asegura un movimiento físico correcto
//        rb.MovePosition(rb.position + currentVelocity * Time.deltaTime);

//        // -------------------------
//        // Spawnear ácido con intervalo fijo
//        acidSpawnTimer += Time.deltaTime;
//        if (acidSpawnTimer >= acidSpawnInterval)
//        {
//            acidSpawnTimer = 0f;
//            GameObject.Instantiate(acidPrefab, enemy.position, Quaternion.identity);
//        }
//        // -------------------------
//    }
//}
