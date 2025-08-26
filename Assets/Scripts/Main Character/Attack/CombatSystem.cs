using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [Header("Combate")]
    public float attackCooldown = 0.3f;
    private float attackTimer;
    public GameObject[] slashEffectPrefabs;

    [Header("Hitbox")]
    public GameObject hitboxPrefab;

    [Header("Ataque")]
    public float hitboxOffset = 0.5f;

    [Header("Recoil")]
    public float dashSpeed = 150f;
    private float dashingSpeed;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private Vector2 attackRecoilDir;

    private int comboIndex = 0;
    private float comboResetTime = 1f;
    private float comboTimer;
    private bool bufferedAttack;

    // Componentes para los nuevos sistemas
    private LaserBeamObserver laserBeamObserver;
    private ThornsPathObserver thornsPathObserver;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
        {
            comboIndex = 0;
            bufferedAttack = false;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            if (IsDashing())
            {
                bufferedAttack = true;
            }
            else if (attackTimer <= 0f)
            {
                PerformAttack();
            }
        }

        if (dashingSpeed > 0f)
        {
            rb.linearVelocity = attackRecoilDir * dashSpeed * Time.deltaTime;

            dashingSpeed = Mathf.Lerp(dashingSpeed, 0f, 15f * Time.deltaTime);

            if (dashingSpeed < 1f)
            {
                dashingSpeed = 0f;
                rb.linearVelocity = Vector2.zero;

                if (playerController != null)
                {
                    playerController.canMove = true;
                }

                if (bufferedAttack)
                {
                    PerformAttack();
                    bufferedAttack = false;
                }
            }
        }
    }

    private void Start()
    {
        // Buscar el observer del laser beam
        laserBeamObserver = LaserBeamObserver.Instance;

        // Buscar el observer del thorns path
        thornsPathObserver = ThornsPathObserver.Instance;
    }

    private void PerformAttack()
    {
        attackTimer = attackCooldown;
        comboTimer = comboResetTime;

        comboIndex++;
        if (comboIndex > 3) comboIndex = 1;

        // Notificar al observer del laser beam cuando se completa el tercer golpe
        if (comboIndex == 3 && laserBeamObserver != null)
        {
            laserBeamObserver.OnComboCompleted(comboIndex, transform);
        }

        // Notificar al observer del thorns path cuando se completa el tercer golpe
        if (comboIndex == 3 && thornsPathObserver != null)
        {
            thornsPathObserver.OnComboCompleted(comboIndex, transform);
        }

        attackRecoilDir = GetAttackDirection();
        dashingSpeed = dashSpeed;

        // Bloquea movimiento durante el ataque
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        // Dispara animación
        if (playerController != null && playerController.animator != null)
        {
            playerController.animator.ResetTrigger("attackTrigger");
            playerController.animator.SetTrigger("attackTrigger");
        }

        // 🎧 Sonido de espada
        AudioManager.Instance.PlayWithRandomPitch("swing", 0.95f, 1.05f);

        // Efecto visual de slash
        Vector2 spawnPos = (Vector2)transform.position + attackRecoilDir * hitboxOffset;
        if (slashEffectPrefabs != null && slashEffectPrefabs.Length >= comboIndex)
        {
            GameObject slashEffectPrefab = slashEffectPrefabs[comboIndex - 1];
            if (slashEffectPrefab != null)
            {
                GameObject slashEffect = Instantiate(slashEffectPrefab, spawnPos, Quaternion.identity, transform);
                slashEffect.transform.right = attackRecoilDir;
                Destroy(slashEffect, 0.2f);
            }
        }

        // Instanciar hitbox de daño
        GameObject hitbox = Instantiate(hitboxPrefab, spawnPos, Quaternion.identity, transform);
        AttackHitbox hitboxScript = hitbox.GetComponent<AttackHitbox>();
        hitboxScript.knockbackDir = attackRecoilDir;

        Debug.Log("Ataque combo " + comboIndex);
    }

    private Vector2 GetAttackDirection()
    {
        Vector2 playerPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;
        Vector2 dir = (mousePos - playerPos).normalized;
        return dir;
    }

    public bool IsDashing()
    {
        return dashingSpeed > 0f;
    }
}