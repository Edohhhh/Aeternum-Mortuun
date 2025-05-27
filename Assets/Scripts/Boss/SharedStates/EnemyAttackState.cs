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

    public EnemyAttackState(Transform enemy)
    {
        this.enemy = enemy;
        this.data = enemy.GetComponent<IEnemyDataProvider>();
        this.rb = enemy.GetComponent<Rigidbody2D>();
        this.spriteRenderer = enemy.GetComponent<SpriteRenderer>();
    }

    public override void Awake()
    {
        base.Awake();
        
    }

    public override void Execute()
    {
        if (data == null || data.GetPlayer() == null) return;

        Vector2 direction = (data.GetPlayer().position - enemy.position).normalized;

        // Movimiento con aceleración
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            direction * data.GetMaxSpeed(),
            data.GetAcceleration() * Time.deltaTime
        );

        rb.MovePosition(rb.position + currentVelocity * Time.deltaTime);
    }
}
//public class EnemyAttackState : State<EnemyInputs>
//{
//private SlimeController slime;
//private Rigidbody2D rb;

//public EnemyAttackState(SlimeController slime)
//{
//    this.slime = slime;
//    rb = slime.GetComponent<Rigidbody2D>();
//}

//public override void Execute()
//{
//    if (slime.GetPlayer() == null) return;

//    // Direcci�n hacia el jugador
//    Vector2 direction = (slime.GetPlayer().position - slime.transform.position).normalized;

//    // Aplicar fuerza como aceleraci�n
//    rb.AddForce(direction * slime.GetAcceleration());

//    // Limitar la velocidad m�xima
//    rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, slime.GetMaxSpeed());

//    // Atacar si est� cerca
//    float distance = Vector2.Distance(slime.transform.position, slime.GetPlayer().position);
//    if (distance <= slime.GetAttackDistance())
//    {
//        Debug.Log("Atacando con da�o: " + slime.GetDamage());
//        // Aqu� podr�as llamar al m�todo de da�o del jugador si lo ten�s
//    }
//}
//}




//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EnemyAttackState : State<EnemyInputs>
//{
//    private Transform enemy;
//    private Transform target;
//    private float speed;

//    public EnemyAttackState(Transform enemy, Transform target, float speed = 2f)
//    {
//        this.enemy = enemy;
//        this.target = target;
//        this.speed = speed;
//    }

//    public override void Execute()
//    {
//        if (target == null) return;

//        // Movimiento simple hacia el jugador
//        Vector3 dir = (target.position - enemy.position).normalized;
//        enemy.position += dir * speed * Time.deltaTime;
//    }
//}


//private SlimeController slime;
//private Transform enemy;
//private Transform target;
//private SpriteRenderer spriteRenderer;
//private Rigidbody2D rb;

//public EnemyAttackState(Transform enemy, Transform target)
//{
//    this.enemy = enemy;
//    this.target = target;
//    this.slime = enemy.GetComponent<SlimeController>();
//    this.rb = enemy.GetComponent<Rigidbody2D>();
//    this.spriteRenderer = enemy.GetComponent<SpriteRenderer>();
//}

//public override void Awake()
//{
//    base.Awake();
//    if (spriteRenderer != null)
//        spriteRenderer.color = Color.red;
//}

//public override void Execute()
//{
//    if (target == null) return;

//    Vector2 dir = (target.position - enemy.position).normalized;

//    // Movimiento con aceleración
//    Vector2 force = dir * slime.GetAcceleration();
//    rb.AddForce(force);

//    // Clamp de velocidad
//    rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, slime.GetMaxSpeed());
//}
