using UnityEngine;

public class DiabloAttack_Walls : IDiabloAttack
{
    public bool IsFinished { get; private set; }

    private DiabloController ctrl;

    private enum Phase { Warning, MovingIn, Gap }
    private Phase phase;

    private float timer;
    private int waveIndex;

    private GameObject leftInst;
    private GameObject rightInst;

    public void Start(DiabloController c)
    {
        ctrl = c;
        IsFinished = false;
        waveIndex = 0;
        StartWarning();

        ctrl.SpawnExtraEnemiesForAttack(4);
    }

    public void Tick(DiabloController c)
    {
        if (IsFinished) return;

        float dt = Time.deltaTime;
        timer += dt;

        switch (phase)
        {
            case Phase.Warning:
                UpdateWarning();
                break;

            case Phase.MovingIn:
                UpdateMoving(dt);
                break;

            case Phase.Gap:
                UpdateGap();
                break;
        }
    }

    public void Stop(DiabloController c)
    {
        DestroyWalls();
        IsFinished = true;
    }

    // ---------- FASE WARNING ----------

    private void StartWarning()
    {
        phase = Phase.Warning;
        timer = 0f;
        Debug.Log($"[DIABLO/Walls] Wave {waveIndex + 1} WARNING");
    }

    private void UpdateWarning()
    {
        if (timer >= ctrl.A4_WarnTime)
        {
            StartWave();
        }
    }

    // ---------- SPAWN DE PAREDES ----------

    private void StartWave()
    {
        timer = 0f;
        phase = Phase.MovingIn;

        if (ctrl.A4_LeftWallPrefab == null ||
            ctrl.A4_RightWallPrefab == null ||
            ctrl.A4_LeftSpawn == null ||
            ctrl.A4_RightSpawn == null)
        {
            Debug.LogWarning("[DIABLO/Walls] Faltan referencias. Cancelando ataque.");
            IsFinished = true;
            return;
        }

        leftInst = Object.Instantiate(
            ctrl.A4_LeftWallPrefab,
            ctrl.A4_LeftSpawn.position,
            ctrl.A4_LeftSpawn.rotation);

        rightInst = Object.Instantiate(
            ctrl.A4_RightWallPrefab,
            ctrl.A4_RightSpawn.position,
            ctrl.A4_RightSpawn.rotation);

        Debug.Log($"[DIABLO/Walls] Spawn walls L={leftInst.transform.position} R={rightInst.transform.position}");
    }

    // ---------- MOVIMIENTO + COLISIÓN ----------

    private void UpdateMoving(float dt)
    {
        if (!leftInst || !rightInst)
        {
            phase = Phase.Gap;
            timer = 0f;
            return;
        }

        // mover hacia el centro
        float speed = ctrl.A4_MoveSpeed;

        Vector3 lp = leftInst.transform.position;
        Vector3 rp = rightInst.transform.position;

        lp.x += speed * dt;
        rp.x -= speed * dt;

        leftInst.transform.position = lp;
        rightInst.transform.position = rp;

        // Comprobamos colisión usando los BoxCollider2D
        var leftCol = leftInst.GetComponent<Collider2D>();
        var rightCol = rightInst.GetComponent<Collider2D>();

        bool collided = false;

        if (leftCol && rightCol)
        {
            Bounds lb = leftCol.bounds;
            Bounds rb = rightCol.bounds;

            // Se tocan cuando el max.x de la izquierda llega al min.x de la derecha
            collided = lb.max.x >= rb.min.x;

            if (collided)
            {
                // Corrige el solapamiento para que queden justo tocándose
                float overlap = lb.max.x - rb.min.x;
                if (overlap > 0f)
                {
                    float half = overlap * 0.5f;

                    lp = leftInst.transform.position;
                    rp = rightInst.transform.position;

                    lp.x -= half;
                    rp.x += half;

                    leftInst.transform.position = lp;
                    rightInst.transform.position = rp;
                }
            }
        }
        else
        {
            // Fallback por si algún prefab no tiene collider
            collided = lp.x >= rp.x;
        }

        if (collided)
        {
            Debug.Log("[DIABLO/Walls] Colliders contact → destroy walls");
            DestroyWalls();
            phase = Phase.Gap;
            timer = 0f;
        }
    }

    // ---------- GAP ENTRE OLAS ----------

    private void UpdateGap()
    {
        if (timer >= ctrl.A4_WaveGap)
        {
            waveIndex++;

            if (waveIndex < ctrl.A4_Waves)
            {
                StartWarning();
            }
            else
            {
                Debug.Log("[DIABLO/Walls] Ataque completo");
                IsFinished = true;
            }
        }
    }

    private void DestroyWalls()
    {
        if (leftInst) Object.Destroy(leftInst);
        if (rightInst) Object.Destroy(rightInst);

        leftInst = null;
        rightInst = null;
    }
}


