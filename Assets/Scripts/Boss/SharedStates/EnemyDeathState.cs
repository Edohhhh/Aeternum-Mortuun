using UnityEngine;

public class EnemyDeathState : State<EnemyInputs>
{
    private readonly MonoBehaviour enemy;
    private readonly float fallbackDuration;
    private float timer;

    private Animator animator;
    private Rigidbody2D rb2d;
    private Collider2D[] colliders;

    private RigidbodyConstraints2D savedConstraints;
    private float savedGravity;
    private bool hadRb;

    public EnemyDeathState(MonoBehaviour enemy, float fallbackDuration = 1.0f)
    {
        this.enemy = enemy;
        this.fallbackDuration = Mathf.Max(0.1f, fallbackDuration);
    }

    public override void Awake()
    {
        base.Awake();

        animator = enemy.GetComponent<Animator>();
        rb2d = enemy.GetComponent<Rigidbody2D>();
        colliders = enemy.GetComponentsInChildren<Collider2D>(true);

        if (rb2d != null)
        {
            hadRb = true;
            savedConstraints = rb2d.constraints;
            savedGravity = rb2d.gravityScale;
            rb2d.linearVelocity = Vector2.zero;   
            rb2d.angularVelocity = 0f;
            rb2d.gravityScale = 0f;
            rb2d.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }

        if (colliders != null)
        {
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Die");
            animator.SetTrigger("Die");
        }

        (enemy as Component).GetComponent<GolemController>()?.RegisterDeathState(this);
        (enemy as Component).GetComponent<SkeletonController>()?.RegisterDeathState(this);
        (enemy as Component).GetComponent<MinionController>()?.RegisterDeathState(this);
        (enemy as Component).GetComponent<TopoController>()?.RegisterDeathState(this);

        timer = 0f;
    }

    public override void Execute()
    {
        timer += Time.deltaTime;
        if (animator == null || timer >= fallbackDuration * 3f)
        {
            SafeDestroy();
        }
    }

    public override void Sleep()
    {
        if (hadRb && rb2d != null)
        {
            rb2d.constraints = savedConstraints;
            rb2d.gravityScale = savedGravity;
            rb2d.linearVelocity = Vector2.zero;  
        }
        base.Sleep();
    }

    public void OnDeathAnimFinished()
    {
        SafeDestroy();
    }

    private void SafeDestroy()
    {
        if (EnemyManager.Instance) EnemyManager.Instance.UnregisterEnemy();
        Object.Destroy(enemy.gameObject);
    }

}


//using System.Collections;
//using UnityEngine;

//public class EnemyDeathState : State<EnemyInputs>
//{
//    private readonly MonoBehaviour enemy;
//    private readonly float duration;
//    private float timer;
//    private Vector3 originalPos;

//    public EnemyDeathState(MonoBehaviour enemy, float duration = 1f)
//    {
//        this.enemy = enemy;
//        this.duration = duration;
//    }

//    public override void Awake()
//    {
//        base.Awake();
//        originalPos = enemy.transform.position;
//        timer = 0f;
//        Debug.Log($"{enemy.name} entró a EnemyDeathState");
//    }

//    public override void Execute()
//    {
//        timer += Time.deltaTime;

//        // Vibración visual
//        float offsetX = Random.Range(-0.1f, 0.1f);
//        float offsetY = Random.Range(-0.1f, 0.1f);
//        enemy.transform.position = originalPos + new Vector3(offsetX, offsetY, 0);

//        if (timer >= duration)
//        {
//            // Restaurar posición
//            enemy.transform.position = originalPos;

//            // Desregistrar y destruir
//            EnemyManager.Instance.UnregisterEnemy();
//            GameObject.Destroy(enemy.gameObject);
//        }
//    }

//    public override void Sleep()
//    {
//        base.Sleep();
//        enemy.transform.position = originalPos;
//    }
//}
