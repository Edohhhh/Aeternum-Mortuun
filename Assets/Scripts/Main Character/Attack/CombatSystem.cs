using UnityEngine;
using System.Collections;

public class CombatSystem : MonoBehaviour
{
    [Header("Configuración Combate")]
    public float attackCooldown = 0.4f;
    // ✅ CAMBIO: Tiempo aumentado a 2 segundos como pediste
    public float comboResetTime = 2f;

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
    private PlayerController playerController;

    public Vector2 LastAttackDir { get; private set; }

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // Cuenta regresiva para olvidar el combo
        if (comboExpireTimer > 0)
        {
            comboExpireTimer -= Time.deltaTime;
            if (comboExpireTimer <= 0)
            {
                // Si pasan 2 segundos sin atacar, volvemos al golpe 1
                comboIndex = 0;
            }
        }
    }

    public void ExecuteAttackLogic(Vector2 dir)
    {
        LastAttackDir = dir;
        // Reiniciamos el reloj de 2 segundos cada vez que atacas
        comboExpireTimer = comboResetTime;

        // 1. Instanciar Hitbox
        if (hitboxPrefab != null)
        {
            Vector2 pos = (Vector2)transform.position + dir * hitboxOffset;
            var hbGo = Instantiate(hitboxPrefab, pos, Quaternion.identity, transform);
            var hb = hbGo.GetComponent<AttackHitbox>();

            if (hb != null)
                hb.Initialize(this, dir, playerController.baseDamage);
        }

        // 2. Instanciar VFX CORRECTO según el combo
        if (slashEffectPrefabs != null && slashEffectPrefabs.Length > 0)
        {
            // Usamos el comboIndex actual
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

    public void OnAttackHit()
    {
        if (playerController != null)
        {
            playerController.OnAttackHitEnemy();
        }
    }

    // ✅ CAMBIO CLAVE: Preparamos el siguiente índice
    // Si tenemos 3 ataques, hará 0 -> 1 -> 2 -> 0 -> 1 ...
    public void AdvanceCombo()
    {
        comboIndex++;
        // Opcional: Si quieres que el combo de 3 golpes se reinicie tras el tercero
        // aunque sigas spameando click, descomenta la siguiente linea.
        // if (slashEffectPrefabs.Length > 0) comboIndex %= slashEffectPrefabs.Length;
    }

    public void ResetCombo() => comboIndex = 0;

    public Vector2 GetAttackDirection()
    {
        Vector2 playerPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;
        Vector2 dir = (mousePos - playerPos).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = playerController.lastNonZeroMoveInput;
        return dir;
    }

    public void TriggerHitStop(float duration)
    {
        if (isStopActive) return;
        StartCoroutine(DoHitStop(duration));
    }

    IEnumerator DoHitStop(float duration)
    {
        isStopActive = true;
        float originalScale = Time.timeScale;
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = originalScale;
        isStopActive = false;
    }
}