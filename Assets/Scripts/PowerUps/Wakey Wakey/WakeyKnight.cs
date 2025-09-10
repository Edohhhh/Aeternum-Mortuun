using System.Collections;
using UnityEngine;

public class WakeyKnight : MonoBehaviour
{
    private Transform player;
    private SpriteRenderer sr;
    private Animator animator;
    private int attackDamage;
    private Vector3 offset = new Vector3(1.5f, 0f, 0f);

    public void Initialize(Transform playerTransform, int damage)
    {
        player = playerTransform;
        attackDamage = damage;

        // Evitar conflictos
        gameObject.tag = "Untagged";
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        animator = GetComponent<Animator>();

        StartCoroutine(FollowAndMimic());
    }

    private IEnumerator FollowAndMimic()
    {
        while (true)
        {
            if (player != null)
            {
                Vector3 targetPos = player.position + offset;
                transform.position = Vector3.Lerp(transform.position, targetPos, 5f * Time.deltaTime);
            }

            yield return new WaitForSeconds(1f);
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (animator) animator.SetTrigger("Attack");

        GameObject hitbox = new GameObject("WakeyKnightHitbox");
        hitbox.transform.position = transform.position + transform.right * 1f;

        var atk = hitbox.AddComponent<WakeyKnightAttack>();
        atk.damage = attackDamage;

        Destroy(hitbox, 0.2f);
    }
}
