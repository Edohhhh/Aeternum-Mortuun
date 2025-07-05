using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class GlobalEnemyDamageObserver : MonoBehaviour
{
    public float slowPercent = 0.05f;
    public float duration = 2f;

    private Dictionary<GameObject, Coroutine> slowCoroutines = new();

    public static void RegisterDamage(GameObject enemyObject)
    {
        var observer =FindAnyObjectByType<GlobalEnemyDamageObserver>();
        if (observer != null && enemyObject.CompareTag("Enemy"))
        {
            observer.TryApplySlow(enemyObject);
        }
    }

    private void TryApplySlow(GameObject enemy)
    {
        if (slowCoroutines.ContainsKey(enemy)) return;

        Coroutine routine = StartCoroutine(ApplySlowViaReflection(enemy));
        slowCoroutines[enemy] = routine;
    }

    private IEnumerator ApplySlowViaReflection(GameObject enemy)
    {
        var controller = enemy.GetComponent<MonoBehaviour>();
        if (controller == null)
        {
            yield break;
        }

        FieldInfo maxSpeedField = controller.GetType().GetField("maxSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (maxSpeedField == null || maxSpeedField.FieldType != typeof(float))
        {
            Debug.LogWarning($"[Slow] {enemy.name} no tiene 'maxSpeed' accesible por reflexión.");
            yield break;
        }

        float originalSpeed = (float)maxSpeedField.GetValue(controller);
        float slowedSpeed = originalSpeed * (1f - slowPercent);

        maxSpeedField.SetValue(controller, slowedSpeed);
        Debug.Log($"[Slow] {enemy.name} ralentizado a {slowedSpeed}");

        yield return new WaitForSeconds(duration);

        if (enemy != null && maxSpeedField != null)
        {
            maxSpeedField.SetValue(controller, originalSpeed);
            Debug.Log($"[Slow] {enemy.name} restaurado a {originalSpeed}");
        }

        slowCoroutines.Remove(enemy);
    }
}
