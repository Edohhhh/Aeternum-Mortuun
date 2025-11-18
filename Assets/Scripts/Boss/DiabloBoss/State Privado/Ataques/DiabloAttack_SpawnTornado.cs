using UnityEngine;

public class DiabloAttack_SpawnTornado : IDiabloAttack
{
    public bool IsFinished { get; private set; }

    private DiabloController ctrl;
    private float t;
    private float life;
    private GameObject tornadoInstance;

    public void Start(DiabloController c)
    {
        ctrl = c;
        IsFinished = false;
        t = 0f;

        // clamp por si en el inspector quedó en 0
        life = Mathf.Max(0.1f, ctrl.A3_TornadoLife);

        // --- Spawn de minions en los waypoints ---
        if (ctrl.A3_MinionPrefab && ctrl.A3_EnemySpawns != null)
        {
            foreach (var sp in ctrl.A3_EnemySpawns)
            {
                if (sp == null) continue;
                Object.Instantiate(ctrl.A3_MinionPrefab, sp.position, sp.rotation);
            }
        }

        // --- Spawn del tornado en el centro ---
        if (ctrl.A3_TornadoPrefab)
        {
            Vector3 pos = ctrl.A3_TornadoCenter
                          ? ctrl.A3_TornadoCenter.position
                          : ctrl.transform.position;

            tornadoInstance = Object.Instantiate(ctrl.A3_TornadoPrefab, pos, Quaternion.identity);
        }
    }

    public void Tick(DiabloController c)
    {
        if (IsFinished) return;

        t += Time.deltaTime;

        // cuando se cumple el tiempo, destruimos el tornado y avisamos que terminó
        if (t >= life)
        {
            if (tornadoInstance)
                Object.Destroy(tornadoInstance);

            Debug.Log("[DIABLO] SpawnTornado END – volver a Idle");
            IsFinished = true;
        }
    }

    public void Stop(DiabloController c)
    {
        // Limpieza por si el ataque se corta antes
        if (tornadoInstance)
            Object.Destroy(tornadoInstance);

        IsFinished = true;
    }
}

