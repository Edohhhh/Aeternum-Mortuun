using UnityEngine;

public class BoneBurstFX : MonoBehaviour
{
    [Header("Particle System principal")]
    [SerializeField] private ParticleSystem burst;

    [Header("Fuerza del estallido")]
    [SerializeField] private float impulse = 10f;      // intensidad de la fuerza
    [SerializeField] private AnimationCurve forceCurve; // 1 al inicio -> 0 rápido

    [Header("Orden de dibujo (opcional)")]
    [SerializeField] private string sortingLayerName = "Effects";
    [SerializeField] private int sortingOrder = 100;

    private void Reset()
    {
        // Curva por defecto: pulso rápido que desaparece
        forceCurve = new AnimationCurve(
            new Keyframe(0.00f, 1f),
            new Keyframe(0.12f, 0.35f),
            new Keyframe(0.25f, 0.0f),
            new Keyframe(1.00f, 0.0f)
        );
    }

    /// <summary>
    /// Llamado al instanciar desde EnemyHealth con la dirección del knockback.
    /// </summary>
    public void Init(Vector2 hitDir)
    {
        if (burst == null) burst = GetComponentInChildren<ParticleSystem>();

        // Dirección contraria al golpe del jugador
        Vector2 outDir = (-hitDir).normalized;
        float ang = Mathf.Atan2(outDir.y, outDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);

        // --- Configuración de módulos para "fuerza sin gravedad" ---
        var main = burst.main;
        main.gravityModifier = 0f;       // sin gravedad
        main.startSpeed = 0f;            // la aceleración la da la fuerza, no la velocidad inicial
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Empuje en +X local (ya rotamos hacia outDir)
        var force = burst.forceOverLifetime;
        force.enabled = true;
        force.space = ParticleSystemSimulationSpace.Local;
        force.x = new ParticleSystem.MinMaxCurve(impulse, forceCurve); // pulso que decae
        force.y = 0f;
        force.z = 0f;

        // (Opcional) Limitar velocidad para que se "paren" y queden tirados
        var limit = burst.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.dampen = 0.6f;      // frena progresivamente
        limit.separateAxes = false;
        limit.limit = new ParticleSystem.MinMaxCurve(6f); // velocidad máx. razonable

        // Mantenerlas un rato y luego desvanecer (hazlo en Color over Lifetime del prefab)
        // Recomendado: alfa 1 hasta ~70% y a 0 al final.

        // Orden de render por encima del mapa (si querés asegurar por código)
        var psr = burst.GetComponent<ParticleSystemRenderer>();
        if (psr != null)
        {
            psr.sortingLayerName = sortingLayerName;
            psr.sortingOrder = sortingOrder;
            psr.renderMode = ParticleSystemRenderMode.Billboard; // o HorizontalBillboard
        }

        burst.Play();

        // Autodestruir contenedor tras finalizar
        float dieAfter = main.duration + main.startLifetime.constantMax + 2f;
        Destroy(gameObject, dieAfter);
    }
}
