using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class PlayerHurtSlowObserver : MonoBehaviour
{
    public float slowPercent = 0.1f;
    public float duration = 2f;

    private Dictionary<GameObject, Coroutine> activeSlows = new();

    public void AttachTo(PlayerController player)
    {
        PlayerDamageHook.Attach(player, this);
    }

    public void OnPlayerDamaged()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (!activeSlows.ContainsKey(enemy))
            {
                Coroutine slowRoutine = StartCoroutine(ApplySlowTo(enemy));
                activeSlows.Add(enemy, slowRoutine);
            }
        }
    }

    private IEnumerator ApplySlowTo(GameObject enemy)
    {
        var controller = enemy.GetComponent<MonoBehaviour>();
        if (controller == null) yield break;

        FieldInfo maxSpeedField = controller.GetType().GetField("maxSpeed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (maxSpeedField == null || maxSpeedField.FieldType != typeof(float)) yield break;

        float originalSpeed = (float)maxSpeedField.GetValue(controller);
        float slowedSpeed = originalSpeed * (1f - slowPercent);
        maxSpeedField.SetValue(controller, slowedSpeed);

        Debug.Log($"[SLOW-PLAYER] {enemy.name} ralentizado por daño al jugador.");

        yield return new WaitForSeconds(duration);

        if (enemy != null)
            maxSpeedField.SetValue(controller, originalSpeed);

        activeSlows.Remove(enemy);
    }
}
