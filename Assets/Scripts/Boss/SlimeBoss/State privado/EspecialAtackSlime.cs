using UnityEngine;

public class EspecialAtackSlime : State<EnemyInputs>
{
    private Transform enemy;
    private SpriteRenderer spriteRenderer;
    private FSM<EnemyInputs> fsm;
    private float prepareDuration = 2f;
    private float timer = 0f;
    private SlimeController slime;
    private Color originalColor;
    private Rigidbody2D rb;
    private IEnemyDataProvider data;

    public EspecialAtackSlime(SlimeController slime, FSM<EnemyInputs> fsm)
    {
        this.slime = slime;
        this.fsm = fsm;
        this.enemy = slime.transform;
        this.data = enemy.GetComponent<IEnemyDataProvider>();
        this.rb = enemy.GetComponent<Rigidbody2D>();
        this.spriteRenderer = enemy.GetComponent<SpriteRenderer>();
    }

    public override void Awake()
    {
        base.Awake();
        

        timer = 0f;

        rb.gravityScale = 0f;       // desactiva la gravedad
        rb.constraints = RigidbodyConstraints2D.FreezeAll; // congela todo

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.blue; // Color azul para fase de preparación
        }

        Debug.Log("Slime: Entró en preparación de ataque especial (EspecialAtackSlime)");
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        // Titileo azul
        if (spriteRenderer != null)
        {
            float lerp = Mathf.PingPong(Time.time * 5f, 1f); // 5f es la velocidad del titileo
            spriteRenderer.color = Color.Lerp(originalColor, Color.blue, lerp);
        }

        if (timer >= prepareDuration)
        {
            fsm.Transition(EnemyInputs.SpecialAttack); // Avanza al ChargeState (que aún falta)
        }
    }

    public override void Sleep()
    {
        base.Sleep();
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 1f; // o el valor que usabas normalmente
    }
}