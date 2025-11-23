using UnityEngine;
using System.Collections;

public class CombatSystem : MonoBehaviour
{
    [Header("Configuración Combate")]
    public float attackCooldown = 0.4f;
    public float comboResetTime = 2f; // Tiempo para mantener el combo

    [Header("Movimiento Ofensivo (Si fallas)")]
    public float recoilDistance = 2f;
    public float recoilDuration = 0.2f;

    [Header("Retroceso al Golpear (Si aciertas)")]
    public float hitRecoilForce = 15f;
    public float hitRecoilDuration = 0.15f;

    [Header("VFX & Hitbox")]
    public GameObject[] slashEffectPrefabs;
    public GameObject hitboxPrefab;
    public float hitboxOffset = 0.8f;

    // Estado interno
    private int comboIndex = 0;
    private float comboExpireTimer;

    private bool isStopActive;
    private Coroutine hitStopCoroutine; // Referencia para matar el freeze si sale la UI

    private PlayerController playerController;
    public Vector2 LastAttackDir { get; private set; }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // Timer para resetear el combo si pasa mucho tiempo
        if (comboExpireTimer > 0)
        {
            comboExpireTimer -= Time.deltaTime;
            if (comboExpireTimer <= 0)
            {
                comboIndex = 0;
            }
        }
    }

    // --- LÓGICA PRINCIPAL LLAMADA POR ATTACKSTATE ---

    public void ExecuteAttackLogic(Vector2 dir)
    {
        LastAttackDir = dir;
        comboExpireTimer = comboResetTime;

        // 1. Instanciar Hitbox
        if (hitboxPrefab != null)
        {
            Vector2 pos = (Vector2)transform.position + dir * hitboxOffset;
            var hbGo = Instantiate(hitboxPrefab, pos, Quaternion.identity, transform);
            var hb = hbGo.GetComponent<AttackHitbox>();

            // Inicializamos la hitbox con daño y referencia
            if (hb != null)
                hb.Initialize(this, dir, playerController.baseDamage);
        }

        // 2. Instanciar VFX del combo actual
        if (slashEffectPrefabs != null && slashEffectPrefabs.Length > 0)
        {
            int idx = comboIndex % slashEffectPrefabs.Length;
            GameObject vfxPrefab = slashEffectPrefabs[idx];
            if (vfxPrefab != null)
            {
                Vector2 pos = (Vector2)transform.position + dir * hitboxOffset;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);

                GameObject vfx = Instantiate(vfxPrefab, pos, rot, transform);
                Destroy(vfx, 0.3f);
            }
        }
    }

    // Llamado cuando la hitbox toca un enemigo
    public void OnAttackHit()
    {
        if (playerController != null)
        {
            playerController.OnAttackHitEnemy();
        }
    }

    public void AdvanceCombo() => comboIndex++;
    public void ResetCombo() => comboIndex = 0;

    public Vector2 GetAttackDirection()
    {
        Vector2 playerPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;
        Vector2 dir = (mousePos - playerPos).normalized;

        // Si el mouse está encima del jugador, usa la última dirección de movimiento
        if (dir.sqrMagnitude < 0.001f) dir = playerController.lastNonZeroMoveInput;

        return dir;
    }

    // --- HITSTOP (FREEZE) ---

    public void TriggerHitStop(float duration)
    {
        if (isStopActive) return;
        hitStopCoroutine = StartCoroutine(DoHitStop(duration));
    }

    IEnumerator DoHitStop(float duration)
    {
        isStopActive = true;
        float originalScale = Time.timeScale;

        // Solo congelamos si el juego no está pausado por la UI
        if (originalScale > 0.01f)
        {
            Time.timeScale = 0.0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = originalScale;
        }

        isStopActive = false;
        hitStopCoroutine = null;
    }

    // ✅ MÉTODO PARA ARREGLAR EL BUG DE LA RULETA
    // La UI llama a esto para asegurarse que el HitStop no descongele el juego
    public void ForceStopCombatForUI()
    {
        if (hitStopCoroutine != null)
        {
            StopCoroutine(hitStopCoroutine);
            hitStopCoroutine = null;
        }
        isStopActive = false;

        // Aseguramos que el tiempo quede en 0 para la UI
        Time.timeScale = 0f;
    }
}