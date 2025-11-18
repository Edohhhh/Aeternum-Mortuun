//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;

//[DisallowMultipleComponent]
//public class RockHardApplier : MonoBehaviour
//{
//    private float originalMoveSpeed;
//    private float originalBaseDamage;

//    private object dashHost;
//    private FieldInfo dashCooldownField;
//    private float originalDashCooldown;
//    private bool dashPatched = false;

//    private PlayerController pc;

//    public void Apply(
//        RockHardDamageMode damageMode, float flatDamage, float expFactor, int expSteps,
//        float moveSpeedDelta, float dashCooldownMultiplier)
//    {
//        pc = GetComponent<PlayerController>();
//        if (pc == null)
//        {
//            Debug.LogWarning("[RockHard] PlayerController no encontrado.");
//            return;
//        }

//        // Guardar originales
//        originalMoveSpeed = pc.moveSpeed;
//        originalBaseDamage = pc.baseDamage;

//        // Daño 
//        if (damageMode == RockHardDamageMode.AddFlat)
//        {
//            pc.baseDamage = (int)(pc.baseDamage + flatDamage); // conversión explícita
//        }
//        else
//        {
//            float mult = Mathf.Pow(expFactor, Mathf.Max(0, expSteps));
//            pc.baseDamage = (int)(pc.baseDamage * mult); // conversión explícita
//        }

//        // Movimiento
//        pc.moveSpeed = (int)Mathf.Max(0f, pc.moveSpeed + moveSpeedDelta); // conversión explícita

//        //Dash cooldown (por reflexión)
//        PatchDashCooldown(dashCooldownMultiplier);

//    }

//    private void PatchDashCooldown(float multiplier)
//    {
//        string[] names = { "dashCooldown", "DashCooldown", "dashCd", "dashDelay", "cooldown" };

//        var comps = GetComponents<MonoBehaviour>();
//        foreach (var c in comps)
//        {
//            if (c == null) continue;
//            var t = c.GetType();
//            foreach (var n in names)
//            {
//                var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//                if (f != null && f.FieldType == typeof(float))
//                {
//                    dashHost = c;
//                    dashCooldownField = f;
//                    originalDashCooldown = (float)dashCooldownField.GetValue(dashHost);
//                    float newCd = originalDashCooldown * Mathf.Max(0.01f, multiplier);
//                    dashCooldownField.SetValue(dashHost, newCd);
//                    dashPatched = true;
//                    return;
//                }
//            }
//        }
//    }

//    private void OnDestroy()
//    {
//        if (pc != null)
//        {
//            pc.moveSpeed = (int)originalMoveSpeed;   // conversión explícita
//            pc.baseDamage = (int)originalBaseDamage; // conversión explícita
//        }

//        if (dashPatched && dashHost != null && dashCooldownField != null)
//        {
//            dashCooldownField.SetValue(dashHost, originalDashCooldown);
//        }
//    }
//}
