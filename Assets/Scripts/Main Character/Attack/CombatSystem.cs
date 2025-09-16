using System.Collections;
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

    [Header("Movimiento durante ataque")]
    public bool allowMovementDuringAttack = false;

    private Rigidbody2D rb;
    private PlayerController playerController;

    private Vector2 lastAttackDir = Vector2.right;
    private Coroutine recoilRoutine;
    private bool bufferedAttack;

    private int comboIndex = 0;
    private float comboResetTime = 1f;
    private float comboTimer;

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
            // 🚫 no atacar si el jugador está dashing
            if (playerController != null && playerController.IsDashing)
                return;

            if (IsRecoiling())
            {
                bufferedAttack = true;
            }
            else if (attackTimer <= 0f)
            {
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        // 🚫 seguridad extra
        if (playerController != null && playerController.IsDashing)
            return;

        attackTimer = attackCooldown;
        comboTimer = comboResetTime;
        comboIndex++;
        if (comboIndex > 3) comboIndex = 1;

        lastAttackDir = GetAttackDirection();

        if (!allowMovementDuringAttack && playerController != null)
            playerController.canMove = false;

        if (playerController != null && playerController.animator != null)
        {
            playerController.animator.ResetTrigger("attackTrigger");
            playerController.animator.SetTrigger("attackTrigger");
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayWithRandomPitch("swing", 0.95f, 1.05f);

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

        StartRecoil(lastAttackDir);
    }

    public void ForceStopRecoil()
    {
        if (recoilRoutine != null)
        {
            StopCoroutine(recoilRoutine);
            recoilRoutine = null;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void StartRecoil(Vector2 dir)
    {
        if (recoilRoutine != null) StopCoroutine(recoilRoutine);
        recoilRoutine = StartCoroutine(Co_Recoil(dir.normalized));
    }

    private IEnumerator Co_Recoil(Vector2 dir)
    {
        int steps = Mathf.Max(1, Mathf.RoundToInt(recoilDuration / Time.fixedDeltaTime));
        float stepDist = recoilDistance / steps;

        rb.linearVelocity = Vector2.zero;

        if (allowMovementDuringAttack)
        {
            for (int i = 0; i < steps; i++)
            {
                rb.MovePosition(rb.position + dir * stepDist);
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            for (int i = 0; i < steps; i++)
            {
                rb.MovePosition(rb.position + dir * stepDist);
                yield return new WaitForFixedUpdate();
            }

            rb.linearVelocity = Vector2.zero;
            if (playerController != null)
                playerController.canMove = true;
        }

        recoilRoutine = null;

        if (bufferedAttack && attackTimer <= 0f)
        {
            bufferedAttack = false;
            PerformAttack();
        }
        else
        {
            bufferedAttack = false;
        }
    }

    private Vector2 GetAttackDirection()
    {
        Vector2 playerPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 mousePos = Input.mousePosition;
        Vector2 dir = (mousePos - playerPos).normalized;

        if (dir.sqrMagnitude < 0.0001f)
            dir = transform.right;

        return dir;
    }

    public bool IsRecoiling()
    {
        return recoilRoutine != null;
    }
}
