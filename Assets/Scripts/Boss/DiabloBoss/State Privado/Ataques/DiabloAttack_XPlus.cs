using UnityEngine;

public class DiabloAttack_XPlus : IDiabloAttack
{
    private enum Phase { WarnX, FireX, Gap, WarnPlus, FirePlus, End }

    private Phase phase;
    private float t;

    private BeamSegment[] beams = null;

    public bool IsFinished { get; private set; }

    // ---- helper interno ----
    private class BeamSegment
    {
        public GameObject go;
        public Transform tr;
        public SpriteRenderer sr;
        public BoxCollider2D col;
        public DevilBeamDamage dmg;
    }

    // Crea UN rayo entre dos puntos
    private BeamSegment SpawnBeamBetween(
        DiabloController ctrl,
        Transform from,
        Transform to,
        float width,
        Color color,
        bool damageOn
    )
    {
        if (!ctrl.A1_BeamPrefab || !from || !to)
            return null;

        var go = Object.Instantiate(ctrl.A1_BeamPrefab, ctrl.transform);
        var tr = go.transform;

        var sr = go.GetComponentInChildren<SpriteRenderer>();
        var col = go.GetComponent<BoxCollider2D>();
        var dmg = go.GetComponent<DevilBeamDamage>();

        Vector3 a = from.position;
        Vector3 b = to.position;
        Vector3 dir = b - a;
        float len = dir.magnitude;

        if (len < 0.001f)
        {
            Object.Destroy(go);
            return null;
        }

        // posición en el medio
        tr.position = (a + b) * 0.5f;
        // rotar para que mire de A hacia B (usamos Vector2.right como referencia)
        tr.rotation = Quaternion.FromToRotation(Vector2.right, dir.normalized);

        // Escala del sprite para que cubra de punta a punta
        float sx = 1f;
        float sy = 1f;

        if (sr && sr.sprite)
        {
            var size = sr.sprite.bounds.size;
            sx = len / Mathf.Max(0.0001f, size.x);
            sy = width / Mathf.Max(0.0001f, size.y);
        }

        tr.localScale = new Vector3(sx, sy, 1f);

        if (sr)
            sr.color = color;

        // Ajustar collider a largo+ancho
        if (col)
        {
            col.size = new Vector2(len, width);
            col.offset = Vector2.zero;
            col.enabled = damageOn;
        }
            
        if (dmg)
        {
            dmg.enabled = damageOn;
            //dmg.damagePerSecond = ctrl.A1_DamagePerSecond;
            //dmg.damageInterval = ctrl.A1_DamageInterval;
            //dmg.targetMask = ctrl.A1_PlayerMask;
        }

        return new BeamSegment
        {
            go = go,
            tr = tr,
            sr = sr,
            col = col,
            dmg = dmg
        };
    }

    private void DestroyBeams()
    {
        if (beams == null) return;

        foreach (var b in beams)
        {
            if (b != null && b.go)
                Object.Destroy(b.go);
        }

        beams = null;
    }

    // ================== IDiabloAttack ==================

    public void Start(DiabloController ctrl)
    {
        IsFinished = false;
        t = 0f;
        phase = Phase.WarnX;

        ctrl.SpawnExtraEnemiesForAttack(1);

        // AVISO de la X: dos rayos diagonales entre las esquinas
        beams = new[]
        {
            SpawnBeamBetween(ctrl, ctrl.A1_TopLeft,     ctrl.A1_BottomRight, ctrl.A1_WarnWidth, ctrl.A1_WarnColor, false),
            SpawnBeamBetween(ctrl, ctrl.A1_TopRight,    ctrl.A1_BottomLeft,  ctrl.A1_WarnWidth, ctrl.A1_WarnColor, false),
        };
    }

    public void Tick(DiabloController ctrl)
    {
        if (IsFinished) return;

        t += Time.deltaTime;

        switch (phase)
        {
            case Phase.WarnX:
                if (t >= ctrl.A1_WarnTime)
                {
                    // destruir aviso y spawnear X de daño
                    DestroyBeams();
                    beams = new[]
                    {
                        SpawnBeamBetween(ctrl, ctrl.A1_TopLeft,  ctrl.A1_BottomRight, ctrl.A1_FireWidth, ctrl.A1_FireColor, true),
                        SpawnBeamBetween(ctrl, ctrl.A1_TopRight, ctrl.A1_BottomLeft,  ctrl.A1_FireWidth, ctrl.A1_FireColor, true),
                    };
                    t = 0f;
                    phase = Phase.FireX;
                }
                break;

            case Phase.FireX:
                if (t >= ctrl.A1_FireTime)
                {
                    DestroyBeams();
                    beams = null;
                    t = 0f;
                    phase = Phase.Gap;
                }
                break;

            case Phase.Gap:
                if (t >= ctrl.A1_GapAfterX)
                {
                    // AVISO del +
                    DestroyBeams();
                    beams = new[]
                    {
                        // vertical (top-bottom)
                        SpawnBeamBetween(ctrl, ctrl.A1_Top,    ctrl.A1_Bottom, ctrl.A1_WarnWidth, ctrl.A1_WarnColor, false),
                        // horizontal (left-right)
                        SpawnBeamBetween(ctrl, ctrl.A1_Left,   ctrl.A1_Right,  ctrl.A1_WarnWidth, ctrl.A1_WarnColor, false),
                    };
                    t = 0f;
                    phase = Phase.WarnPlus;
                }
                break;

            case Phase.WarnPlus:
                if (t >= ctrl.A1_WarnTime)
                {
                    // destruir aviso y spawnear + de daño
                    DestroyBeams();
                    beams = new[]
                    {
                        SpawnBeamBetween(ctrl, ctrl.A1_Top,  ctrl.A1_Bottom, ctrl.A1_FireWidth, ctrl.A1_FireColor, true),
                        SpawnBeamBetween(ctrl, ctrl.A1_Left, ctrl.A1_Right,  ctrl.A1_FireWidth, ctrl.A1_FireColor, true),
                    };
                    t = 0f;
                    phase = Phase.FirePlus;
                }
                break;

            case Phase.FirePlus:
                if (t >= ctrl.A1_FireTime)
                {
                    DestroyBeams();
                    beams = null;
                    phase = Phase.End;
                    IsFinished = true;
                }
                break;
        }
    }

    public void Stop(DiabloController ctrl)
    {
        DestroyBeams();
        IsFinished = true;
    }
}
