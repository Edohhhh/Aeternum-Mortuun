using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class TornadoPull : MonoBehaviour
{
    [Header("Atracción")]
    [SerializeField] private float pullForce = 15f;
    [SerializeField] private float maxDistanceFactor = 1f;
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = true;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        var prb = other.attachedRigidbody;
        if (prb == null) return;

        // dirección HACIA EL TORNADO
        Vector2 dir = (transform.position - other.transform.position);

        // fuerza aumenta cuando está lejos y baja cuando está cerca
        float dist = dir.magnitude;
        float scaledForce = pullForce * Mathf.Clamp(dist * maxDistanceFactor, 0.3f, 2f);

        prb.AddForce(dir.normalized * scaledForce, ForceMode2D.Force);
    }
}

