using UnityEngine;

public class ShieldIconFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);
    public float bobAmplitude = 0.08f;
    public float bobSpeed = 3f;

    private void LateUpdate()
    {
        if (target == null) { Destroy(gameObject); return; }
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = target.position + offset + new Vector3(0f, bob, 0f);
    }
}
