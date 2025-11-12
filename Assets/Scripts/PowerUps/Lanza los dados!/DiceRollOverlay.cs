using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

/// <summary>
/// Overlay temporal que:
/// 1) Anima y muestra 3 tiradas (Daño, Velocidad, Vida) con valores {-3,-2,-1,1,2,3}.
/// 2) Aplica los resultados en la escena actual (con clamps a mínimos).
/// 3) Registra los deltas en un Keeper persistente para re-aplicar en futuras escenas.
/// 4) Se autodestruye y borra el marcador.
/// </summary>
public class DiceRollOverlay : MonoBehaviour
{
    private PlayerController player;
    private int[] faces;
    private float animTimePerStat;
    private float finalHold;
    private GameObject marker;

    private enum Step { RollingDamage, RollingSpeed, RollingHealth, ShowingFinal, Done }
    private Step step = Step.RollingDamage;

    private int currentShown = 0;
    private int resultDamage = 0;
    private int resultSpeed = 0;
    private int resultHealth = 0;

    private GUIStyle bigStyle;
    private GUIStyle labelStyle;
    private float screenScale = 1f;

    public void Initialize(PlayerController player, int[] faces, float perStatAnim, float showFinalFor, GameObject marker)
    {
        this.player = player;
        this.faces = (faces != null && faces.Length > 0) ? faces : new int[] { -3, -2, -1, 1, 2, 3 };
        this.animTimePerStat = Mathf.Max(0.1f, perStatAnim);
        this.finalHold = Mathf.Max(0.5f, showFinalFor);
        this.marker = marker;

        DontDestroyOnLoad(gameObject);
        StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        // DAÑO
        yield return StartCoroutine(ShuffleThenPick(v => resultDamage = v));
        step = Step.RollingSpeed;

        // VELOCIDAD
        yield return StartCoroutine(ShuffleThenPick(v => resultSpeed = v));
        step = Step.RollingHealth;

        // VIDA
        yield return StartCoroutine(ShuffleThenPick(v => resultHealth = v));

        // Aplicar en escena actual
        ApplyAllInCurrentScene();

        // Registrar en Keeper persistente (para futuras escenas)
        var keeperGO = GameObject.Find("DiceRNGKeeper");
        DiceRNGKeeper keeper;
        if (keeperGO == null)
        {
            keeperGO = new GameObject("DiceRNGKeeper");
            keeper = keeperGO.AddComponent<DiceRNGKeeper>();
        }
        else
        {
            keeper = keeperGO.GetComponent<DiceRNGKeeper>();
        }
        keeper.RecordDeltasForThisScene(resultDamage, resultSpeed, resultHealth);

        // Mostrar resultado final breve
        step = Step.ShowingFinal;
        yield return new WaitForSeconds(finalHold);

        step = Step.Done;

        if (marker != null) Object.Destroy(marker);
        Destroy(gameObject);
    }

    private IEnumerator ShuffleThenPick(System.Action<int> onFinish)
    {
        float t = 0f;
        while (t < animTimePerStat)
        {
            currentShown = faces[Random.Range(0, faces.Length)];
            yield return new WaitForSeconds(0.06f);
            t += 0.06f;
        }
        int picked = faces[Random.Range(0, faces.Length)];
        currentShown = picked;
        onFinish?.Invoke(picked);
    }

    private void ApplyAllInCurrentScene()
    {
        if (player == null) return;

        // === DAÑO (mínimo 1) ===
        int base0 = player.baseDamage;
        player.baseDamage = Mathf.Max(1, base0 + resultDamage);

        // === VELOCIDAD (mínimo 1) ===
        float sp0 = player.moveSpeed;
        player.moveSpeed = Mathf.Max(1f, sp0 + resultSpeed);

        // === VIDA (ajusta max y current) ===
        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            float max0 = ph.maxHealth;
            ph.maxHealth = Mathf.Max(1f, max0 + resultHealth);

            if (resultHealth > 0)
                ph.currentHealth = Mathf.Min(ph.currentHealth + resultHealth, ph.maxHealth);
            else
                ph.currentHealth = Mathf.Min(ph.currentHealth, ph.maxHealth);

            if (ph.healthUI != null)
            {
                ph.healthUI.Initialize(ph.maxHealth);
                ph.healthUI.UpdateHearts(ph.currentHealth);
            }
        }

        Debug.Log($"🎲 [DiceRNG] Resultado => ΔDaño {resultDamage}, ΔVel {resultSpeed}, ΔVida {resultHealth}");
    }

    private void OnGUI()
    {
        if (step == Step.Done) return;

        if (bigStyle == null)
        {
            bigStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = true };
            labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            // Escala según resolución
            screenScale = Mathf.Clamp((float)Screen.height / 1080f, 0.7f, 1.6f);
            bigStyle.fontSize = Mathf.RoundToInt(90 * screenScale);
            labelStyle.fontSize = Mathf.RoundToInt(30 * screenScale);
        }

        float w = 700 * screenScale;
        float h = 300 * screenScale;
        Rect r = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);

        // Fondo translúcido
        Color old = GUI.color;
        GUI.color = new Color(0, 0, 0, 0.45f);
        GUI.Box(r, GUIContent.none);
        GUI.color = old;

        // Título
        string title = step switch
        {
            Step.RollingDamage => "Tirando para DAÑO…",
            Step.RollingSpeed => "Tirando para VELOCIDAD…",
            Step.RollingHealth => "Tirando para VIDA…",
            Step.ShowingFinal => "Resultado final",
            _ => "Dados"
        };
        GUI.Label(new Rect(r.x, r.y + 10, r.width, 40 * screenScale), title, labelStyle);

        // Valor grande con ajuste de fuente en el final para que entre
        string valueText;
        if (step == Step.ShowingFinal)
        {
            valueText = $"Daño: {resultDamage}   |   Velocidad: {resultSpeed}   |   Vida: {resultHealth}";
            bigStyle.fontSize = Mathf.RoundToInt(44 * screenScale); // más chico para que entre
        }
        else
        {
            valueText = currentShown.ToString();
            bigStyle.fontSize = Mathf.RoundToInt(90 * screenScale); // grande mientras rueda
        }

        Rect textRect = new Rect(r.x + 14, r.y + (h * 0.25f), r.width - 28, h * 0.55f);
        GUI.Label(textRect, valueText, bigStyle);
    }
}
