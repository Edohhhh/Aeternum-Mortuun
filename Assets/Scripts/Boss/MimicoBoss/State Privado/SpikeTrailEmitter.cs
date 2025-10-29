using System.Collections;
using UnityEngine;

public class SpikeTrailEmitter : MonoBehaviour
{
    public GameObject spikeTrapPrefab;
    public float trailStep = 0.6f;
    public float trapDuration = 8f;
    public float moveSpeed = 8f;

    private Vector3 start;
    private Vector3 end;

    public void Init(Vector3 from, Vector3 to, GameObject trapPrefab, float step, float duration, float speed)
    {
        start = from; end = to;
        spikeTrapPrefab = trapPrefab;
        trailStep = step;
        trapDuration = duration;
        moveSpeed = speed;
        transform.position = start;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        float dist = Vector3.Distance(start, end);
        float t = 0f;
        Vector3 dir = (end - start).normalized;
        float traveled = 0f;
        float nextDrop = 0f;

        // primer drop
        DropTrap(transform.position);

        while (traveled < dist)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position += dir * step;
            traveled += step;

            if (traveled >= nextDrop)
            {
                DropTrap(transform.position);
                nextDrop += trailStep;
            }
            yield return null;
        }

        // último drop en el borde
        DropTrap(end);
        Destroy(gameObject);
    }

    private void DropTrap(Vector3 pos)
    {
        if (!spikeTrapPrefab) return;
        var trap = Instantiate(spikeTrapPrefab, pos, Quaternion.identity);
        Destroy(trap, trapDuration);
    }
}

