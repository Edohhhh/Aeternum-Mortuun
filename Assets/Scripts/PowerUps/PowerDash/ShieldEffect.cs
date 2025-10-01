using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    private float timer;
    private bool active = false;
    private SpriteRenderer sr;
    private Color originalColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        // Si no existe ya el observer, se agrega
        if (GetComponent<PlayerShieldObserver>() == null)
        {
            var obs = gameObject.AddComponent<PlayerShieldObserver>();
            obs.playerHealth = GetComponent<PlayerHealth>();
        }
    }

    public void ActivateShield(float duration)
    {
        timer = duration;
        active = true;

        if (sr != null)
            sr.color = Color.cyan;
    }

    void Update()
    {
        if (!active) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            active = false;
            if (sr != null)
                sr.color = originalColor;

            // opcional: destruir escudo cuando termina
            Destroy(this);
        }
    }

    public bool IsShieldActive() => active;
}
