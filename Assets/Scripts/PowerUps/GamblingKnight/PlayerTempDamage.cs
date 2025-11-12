using UnityEngine;
using System.Collections;

public class PlayerTempDamage : MonoBehaviour
{
    [Tooltip("+/- daño a aplicar temporalmente")]
    public int delta = 1;

    [Tooltip("Duración en segundos")]
    public float duration = 5f;

    private PlayerController pc;
    private bool applied;

    private void OnEnable()
    {
        pc = GetComponent<PlayerController>();
        if (pc == null) return;

        pc.baseDamage += delta;
        applied = true;

        StartCoroutine(Lifetime());
    }

    private IEnumerator Lifetime()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(this);
    }

    private void OnDestroy()
    {
        if (applied && pc != null)
        {
            pc.baseDamage -= delta;
        }
    }
}
