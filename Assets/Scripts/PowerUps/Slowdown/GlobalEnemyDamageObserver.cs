using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class GlobalEnemyDamageObserver : MonoBehaviour
{
    [Header("Slow Settings")]
    public float slowPercent = 0.05f;
    public float duration = 2f;

    [Header("VFX")]
    public GameObject slowEffectPrefab; // <-- referencia al prefab

    private Dictionary<GameObject, Coroutine> slowCoroutines = new();

    public static void RegisterDamage(GameObject enemyObject)
    {
        var observer = FindAnyObjectByType<GlobalEnemyDamageObserver>();
        if (observer != null && enemyObject.CompareTag("Enemy"))
        {
            observer.TryApplySlow(enemyObject);
        }
    }

    private void TryApplySlow(GameObject enemy)
    {
        // Si ya est? ralentizado, no lo aplicamos de nuevo
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

        FieldInfo maxSpeedField = controller.GetType().GetField(
            "maxSpeed",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (maxSpeedField == null || maxSpeedField.FieldType != typeof(float))
        {
            Debug.LogWarning($"[Slow] {enemy.name} no tiene 'maxSpeed' accesible por reflexi?n.");
            yield break;
        }

        float originalSpeed = (float)maxSpeedField.GetValue(controller);
        float slowedSpeed = originalSpeed * (1f - slowPercent);
        maxSpeedField.SetValue(controller, slowedSpeed);
        Debug.Log($"[Slow] {enemy.name} ralentizado a {slowedSpeed}");

        // ?? Instanciar el prefab de la part?cula sobre el enemigo
        GameObject vfxInstance = null;
        if (slowEffectPrefab != null && enemy != null)
        {
            vfxInstance = Instantiate(
                slowEffectPrefab,
                enemy.transform.position,
                Quaternion.identity,
                enemy.transform   // lo parentamos al enemigo para que lo siga
            );
        }

        // Mantener el slow por la duraci?n
        yield return new WaitForSeconds(duration);

        // Restaurar velocidad
        if (enemy != null && maxSpeedField != null)
        {
            maxSpeedField.SetValue(controller, originalSpeed);
            Debug.Log($"[Slow] {enemy.name} restaurado a {originalSpeed}");
        }

        // Destruir el VFX si sigue existiendo
        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }

        slowCoroutines.Remove(enemy);
    }
}
