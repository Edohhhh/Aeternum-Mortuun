//using UnityEngine;
//using System.Collections;

//public class EnemyStunState : State<EnemyInputs>
//{
//    private Transform enemy;
//    private SpriteRenderer spriteRenderer;
//    private float stunDuration = 2f;
//    private float timer = 0f;
//    private Coroutine flashCoroutine;

//    public EnemyStunState(Transform enemy)
//    {
//        this.enemy = enemy;
//    }

//    public override void Awake()
//    {
//        base.Awake();
//        spriteRenderer = enemy.GetComponent<SpriteRenderer>();
//        timer = 0f;

//        // Iniciar titileo
//        if (spriteRenderer != null)
//        {
//            var mono = enemy.GetComponent<MonoBehaviour>();
//            flashCoroutine = mono.StartCoroutine(FlashWhite());
//        }

//        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
//        if (rb != null)
//            rb.linearVelocity = Vector2.zero;
//    }

//    public override void Execute()
//    {
//        timer += Time.deltaTime;
//        if (timer >= stunDuration)
//        {
//            var controller = enemy.GetComponent<IEnemyDataProvider>();
//            var stunnable = enemy.GetComponent<IStunnable>();
//            stunnable?.EndStun();

//            if (controller != null && controller.GetPlayer() != null)
//            {
//                float distance = Vector2.Distance(enemy.position, controller.GetPlayer().position);
//                if (distance <= controller.GetDetectionRadius())
//                    controller.Transition(EnemyInputs.SeePlayer);
//                else
//                    controller.Transition(EnemyInputs.LostPlayer);
//            }
//        }
//    }

//    public override void Sleep()
//    {
//        base.Sleep();
//        if (spriteRenderer != null && flashCoroutine != null)
//        {
//            var mono = enemy.GetComponent<MonoBehaviour>();
//            mono.StopCoroutine(flashCoroutine);
//            spriteRenderer.color = Color.white;
//        }
//    }

//    private IEnumerator FlashWhite()
//    {
//        while (true)
//        {
//            spriteRenderer.color = Color.white;
//            yield return new WaitForSeconds(0.1f);
//            spriteRenderer.color = Color.red;
//            yield return new WaitForSeconds(0.1f);
//        }
//    }
//}