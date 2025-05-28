using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WeaponDebugVisualizer : MonoBehaviour
{
    public Transform attackPoint;

    [Header("Debug Settings")]
    public bool drawGizmos = true;

    [Tooltip("Dirección simulada del ataque (por ejemplo, (1,0) para derecha)")]
    public Vector2 debugDir = Vector2.right;
    public float debugRange = 1f;
    public float debugRadius = 0.5f;

    [Header("Espada Visual (WeaponRenderer)")]
    public Transform weaponTransform;
    public Vector2 offsetUp = new Vector2(0, 0.4f);
    public Vector2 offsetDown = new Vector2(0, -0.4f);
    public Vector2 offsetRight = new Vector2(0.5f, 0);
    public Vector2 offsetLeft = new Vector2(-0.5f, 0);
    public string directionPreview = "right"; // up, down, left, right

    void OnDrawGizmos()
    {
        if (!drawGizmos || attackPoint == null) return;

        // Slash hitbox visual
        Vector2 attackPos = (Vector2)attackPoint.position + debugDir.normalized * debugRange;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, debugRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(attackPoint.position, attackPos);

        // Espada preview visual
        if (weaponTransform != null)
        {
            Vector2 weaponOffset = Vector2.zero;
            float rot = 0f;

            switch (directionPreview.ToLower())
            {
                case "up":
                    weaponOffset = offsetUp;
                    rot = 90f;
                    break;
                case "down":
                    weaponOffset = offsetDown;
                    rot = -90f;
                    break;
                case "left":
                    weaponOffset = offsetLeft;
                    rot = 180f;
                    break;
                case "right":
                    weaponOffset = offsetRight;
                    rot = 0f;
                    break;
            }

            Vector3 swordPos = transform.position + (Vector3)weaponOffset;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(swordPos, 0.1f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, swordPos);

#if UNITY_EDITOR
            Handles.color = Color.white;
            Handles.ArrowHandleCap(0, swordPos, Quaternion.Euler(0, 0, rot), 0.5f, EventType.Repaint);
#endif
        }
    }
}