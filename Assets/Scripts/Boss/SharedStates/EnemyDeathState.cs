using System.Collections;
using UnityEngine;

public class EnemyDeathState : State<EnemyInputs>
{
    private readonly MonoBehaviour enemy;
    private readonly float duration;
    private float timer;
    private Vector3 originalPos;

    public EnemyDeathState(MonoBehaviour enemy, float duration = 1f)
    {
        this.enemy = enemy;
        this.duration = duration;
    }

    public override void Awake()
    {
        base.Awake();
        originalPos = enemy.transform.position;
        timer = 0f;
        Debug.Log($"{enemy.name} entró a EnemyDeathState");
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        // Vibración visual
        float offsetX = Random.Range(-0.1f, 0.1f);
        float offsetY = Random.Range(-0.1f, 0.1f);
        enemy.transform.position = originalPos + new Vector3(offsetX, offsetY, 0);

        if (timer >= duration)
        {
            // Restaurar posición
            enemy.transform.position = originalPos;

            // Desregistrar y destruir
            EnemyManager.Instance.UnregisterEnemy();
            GameObject.Destroy(enemy.gameObject);
        }
    }

    public override void Sleep()
    {
        base.Sleep();
        enemy.transform.position = originalPos;
    }
}
