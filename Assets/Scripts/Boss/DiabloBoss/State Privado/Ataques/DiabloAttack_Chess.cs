using UnityEngine;

public class DiabloAttack_Chess : IDiabloAttack
{
    private enum Phase { Build, Warn, Wave, Gap, ReWarn, End }

    public bool IsFinished { get; private set; }

    private DiabloController ctrl;
    private Phase phase;
    private float t;
    private int waveIndex;
    private bool evenParity; // true => (i+j)%2==0

    // grid
    private Rect worldRect;
    private int rows, cols;
    private Vector2 cellSize;

    // pooling
    private GameObject[,] warnTiles;
    private GameObject[,] pillars;

    // sprite blanco unitario para tiles/pilares
    private static Sprite _whiteSprite;
    private static Sprite WhiteSprite
    {
        get
        {
            if (_whiteSprite == null)
            {
                var tex = Texture2D.whiteTexture;
                _whiteSprite = Sprite.Create(
                    tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f
                );
            }
            return _whiteSprite;
        }
    }

    public void Start(DiabloController c)
    {
        ctrl = c;
        IsFinished = false;
        t = 0f;
        waveIndex = 0;
        evenParity = true; // podés randomizar si querés
        phase = Phase.Build;

        ctrl.SpawnExtraEnemiesForAttack(2);
    }

    public void Tick(DiabloController c)
    {
        switch (phase)
        {
            case Phase.Build:
                BuildGrid();
                ShowWarn(evenParity, true);
                t = 0f;
                phase = Phase.Warn;
                break;

            case Phase.Warn:
                t += Time.deltaTime;
                if (t >= ctrl.Ch_WarnTime)
                {
                    ShowWarn(evenParity, false);
                    FireWave(evenParity, true);  // rayos ON
                    t = 0f;
                    phase = Phase.Wave;
                }
                break;

            case Phase.Wave:
                t += Time.deltaTime;
                if (t >= ctrl.Ch_FireTime)
                {
                    FireWave(evenParity, false); // rayos OFF
                    t = 0f;
                    waveIndex++;

                    if (waveIndex >= ctrl.Ch_Waves)
                    {
                        CleanupAll();
                        IsFinished = true;
                        phase = Phase.End;
                    }
                    else
                    {
                        if (ctrl.Ch_AlternateParity) evenParity = !evenParity;
                        phase = ctrl.Ch_ReWarnEachWave ? Phase.ReWarn : Phase.Gap;
                    }
                }
                break;

            case Phase.ReWarn: // mini aviso opcional antes de cada oleada
                ShowWarn(evenParity, true);
                t += Time.deltaTime;
                if (t >= ctrl.Ch_ReWarnTime)
                {
                    ShowWarn(evenParity, false);
                    FireWave(evenParity, true);
                    t = 0f;
                    phase = Phase.Wave;
                }
                break;

            case Phase.Gap:
                t += Time.deltaTime;
                if (t >= ctrl.Ch_WaveGap)
                {
                    FireWave(evenParity, true);
                    t = 0f;
                    phase = Phase.Wave;
                }
                break;

            case Phase.End:
                break;
        }
    }

    public void Stop(DiabloController c)
    {
        CleanupAll();
        IsFinished = true;
    }

    // ======================= helpers =======================

    private void BuildGrid()
    {
        // 1) calcular rectángulo de arena en mundo
        if (ctrl.Ch_UseArenaCollider && ctrl.Ch_ArenaCollider != null)
        {
            var b = ctrl.Ch_ArenaCollider.bounds;
            worldRect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
        }
        else
        {
            worldRect = new Rect(
                ctrl.Ch_ManualCenter.x - ctrl.Ch_ManualSize.x * 0.5f,
                ctrl.Ch_ManualCenter.y - ctrl.Ch_ManualSize.y * 0.5f,
                ctrl.Ch_ManualSize.x, ctrl.Ch_ManualSize.y
            );
        }

        rows = Mathf.Max(1, ctrl.Ch_Rows);
        cols = Mathf.Max(1, ctrl.Ch_Cols);
        cellSize = new Vector2(worldRect.width / cols, worldRect.height / rows);

        bool needRebuild = warnTiles == null
                        || warnTiles.GetLength(0) != rows
                        || warnTiles.GetLength(1) != cols;

        if (needRebuild)
        {
            CleanupAllImmediate();

            warnTiles = new GameObject[rows, cols];
            pillars = new GameObject[rows, cols];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    // ----- AVISO -----
                    var wt = Object.Instantiate(ctrl.ChessWarnPrefab, ctrl.transform);
                    wt.name = $"ChessWarn_{i}_{j}";
                    wt.transform.localScale = Vector3.one;
                    wt.SetActive(false);
                    warnTiles[i, j] = wt;

                    // ----- PILAR (con collider + daño del prefab) -----
                    var pl = Object.Instantiate(ctrl.ChessFirePrefab, ctrl.transform);
                    pl.name = $"ChessPillar_{i}_{j}";
                    pl.transform.localScale = Vector3.one;
                    pl.SetActive(false);
                    pillars[i, j] = pl;
                }
        }

        // 2) posicionar y ESCALAR según el tamaño de la celda (no tocamos colliders)
        float pad = ctrl.Ch_TilePadding;
        Vector2 targetSize = new Vector2(
            Mathf.Max(0.01f, cellSize.x - pad),
            Mathf.Max(0.01f, cellSize.y - pad)
        );

        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                var center = GetCellCenter(i, j);

                // -------- WARN --------
                var wt = warnTiles[i, j];
                wt.transform.position = center;
                wt.transform.rotation = Quaternion.identity;

                var srW = wt.GetComponent<SpriteRenderer>();
                if (srW)
                {
                    srW.color = ctrl.Ch_WarnColor;

                    // tamaño del sprite en unidades de mundo
                    Vector2 spriteSize = Vector2.one;
                    if (srW.sprite != null)
                    {
                        var b = srW.sprite.bounds.size;
                        spriteSize = new Vector2(b.x, b.y);
                    }

                    // escala uniforme para NO deformar el sprite
                    float scaleX = targetSize.x / Mathf.Max(0.0001f, spriteSize.x);
                    float scaleY = targetSize.y / Mathf.Max(0.0001f, spriteSize.y);
                    float scale = Mathf.Min(scaleX, scaleY);   // usamos el más chico

                    wt.transform.localScale = new Vector3(scale, scale, 1f);
                }

                // -------- PILLAR --------
                var pl = pillars[i, j];
                pl.transform.position = center;
                pl.transform.rotation = Quaternion.identity;

                var srP = pl.GetComponent<SpriteRenderer>();
                if (srP)
                {
                    srP.color = ctrl.Ch_FireColor;

                    Vector2 spriteSize = Vector2.one;
                    if (srP.sprite != null)
                    {
                        var b = srP.sprite.bounds.size;
                        spriteSize = new Vector2(b.x, b.y);
                    }

                    float scaleX = targetSize.x / Mathf.Max(0.0001f, spriteSize.x);
                    float scaleY = targetSize.y / Mathf.Max(0.0001f, spriteSize.y);
                    float scale = Mathf.Min(scaleX, scaleY);   // mismo truco

                    pl.transform.localScale = new Vector3(scale, scale, 1f);
                }
                // OJO: NO tocamos BoxCollider.size ni DevilBeamDamage acá.
            }
    }

    private Vector3 GetCellCenter(int i, int j)
    {
        float x = worldRect.xMin + (j + 0.5f) * cellSize.x;
        float y = worldRect.yMin + (i + 0.5f) * cellSize.y;
        return new Vector3(x, y, 0f);
    }

    private bool IsActiveCell(int i, int j, bool even) => ((i + j) % 2 == 0) == even;

    private void ShowWarn(bool even, bool on)
    {
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                warnTiles[i, j].SetActive(on && IsActiveCell(i, j, even));
    }

    private void FireWave(bool even, bool on)
    {
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
            {
                bool active = on && IsActiveCell(i, j, even);
                var go = pillars[i, j];
                go.SetActive(active);
                // al hacer SetActive, el prefab se encarga de collider + daño
            }
    }

    private void CleanupAll()
    {
        if (warnTiles != null)
            foreach (var t in warnTiles)
                if (t) t.SetActive(false);

        if (pillars != null)
            foreach (var p in pillars)
                if (p) p.SetActive(false);
    }

    private void CleanupAllImmediate()
    {
        if (warnTiles != null) { foreach (var t in warnTiles) if (t) Object.Destroy(t); warnTiles = null; }
        if (pillars != null) { foreach (var p in pillars) if (p) Object.Destroy(p); pillars = null; }
    }
}

