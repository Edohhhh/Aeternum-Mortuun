using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [Header("Combate")]
    public float attackCooldown = 0.30f;
    private float attackTimer;

    [Header("Efectos / Hitbox")]
    public GameObject[] slashEffectPrefabs;
    public GameObject hitboxPrefab;
    public float hitboxOffset = 0.5f;

    [Header("Recoil")]
    public float recoilDistance = 0.22f;
    public float recoilDuration = 0.07f;

    private PlayerController playerController;
    private Vector2 lastAttackDir = Vector2.right;
    public Vector2 LastAttackDir => lastAttackDir;

    private int comboIndex = 0;
    private float comboResetTime = 1f;
    private float comboTimer;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;
        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
            comboIndex = 0;

        if (Input.GetButtonDown("Fire1"))
        {
            // 🚫 No permitir atacar si estoy en recoil
            if (playerController.stateMachine.CurrentState == playerController.RecoilState)
                return;

            if (attackTimer <= 0f)
                PerformAttack();
        }
    }

    private void PerformAttack()
    {
        attackTimer = attackCooldown;
        comboTimer = comboResetTime;
        comboIndex++;
        if (comboIndex > 3) comboIndex = 1;

        lastAttackDir = GetAttackDirection();

        // Hitbox / efectos
        Vector2 spawnPos = (Vector2)transform.position + lastAttackDir * hitboxOffset;
        if (slashEffectPrefabs != null && slashEffectPrefabs.Length >= comboIndex)
        {
            var slashPrefab = slashEffectPrefabs[comboIndex - 1];
            if (slashPrefab != null)
            {
                var slash = Instantiate(slashPrefab, spawnPos, Quaternion.identity, transform);
                slash.transform.right = lastAttackDir;
                Destroy(slash, 0.25f);
            }
        }

        if (hitboxPrefab != null)
        {
            var hitbox = Instantiate(hitboxPrefab, spawnPos, Quaternion.identity, transform);
            var hitboxScript = hitbox.GetComponent<AttackHitbox>();
            if (hitboxScript != null)
                hitboxScript.knockbackDir = lastAttackDir;
        }

        // 🔥 Entrar al estado recoil
        playerController.stateMachine.ChangeState(playerController.RecoilState);
    }

    private Vector2 GetAttackDirection()
    {
        Vector2 playerPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;
        Vector2 dir = (mousePos - playerPos).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = transform.right;
        return dir;
    }
}
