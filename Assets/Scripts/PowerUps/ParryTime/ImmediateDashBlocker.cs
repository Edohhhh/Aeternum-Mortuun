using UnityEngine;
using System.Reflection;

[DisallowMultipleComponent]
public class ImmediateDashBlocker : MonoBehaviour
{
    public float hugeCooldown = 999f;

    private object dashHost;
    private FieldInfo dashBoolField;
    private FieldInfo dashCdField;
    private bool dashBoolPatched = false;
    private bool originalDashBool = true;
    private float originalDashCd;

    private void Awake()
    {
        TryBlockNow();
    }

    public void TryBlockNow()
    {
        var comps = GetComponents<MonoBehaviour>();

        // Intentar bool tipo canDash/dashEnabled
        string[] boolNames = { "canDash", "dashEnabled", "dashAllow", "enableDash" };
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();
            foreach (var name in boolNames)
            {
                var f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null && f.FieldType == typeof(bool))
                {
                    dashHost = c;
                    dashBoolField = f;
                    originalDashBool = (bool)dashBoolField.GetValue(dashHost);
                    dashBoolField.SetValue(dashHost, false);
                    dashBoolPatched = true;
                    dashCdField = null;
                    return;
                }
            }
        }

        // Si no hay bool, forzar cooldown enorme
        string[] cdNames = { "dashCooldown", "DashCooldown", "dashCd", "dashDelay", "cooldown" };
        foreach (var c in comps)
        {
            if (c == null) continue;
            var t = c.GetType();
            foreach (var n in cdNames)
            {
                var f = t.GetField(n, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null && f.FieldType == typeof(float))
                {
                    dashHost = c;
                    dashCdField = f;
                    originalDashCd = (float)dashCdField.GetValue(dashHost);
                    dashCdField.SetValue(dashHost, hugeCooldown);
                    dashBoolPatched = false;
                    return;
                }
            }
        }
    }

    private void OnDestroy()
    {
        // revertir si fuera necesario (opcional)
        if (dashBoolPatched && dashHost != null && dashBoolField != null)
        {
            dashBoolField.SetValue(dashHost, originalDashBool);
        }
        else if (dashCdField != null && dashHost != null)
        {
            dashCdField.SetValue(dashHost, originalDashCd);
        }
    }
}
