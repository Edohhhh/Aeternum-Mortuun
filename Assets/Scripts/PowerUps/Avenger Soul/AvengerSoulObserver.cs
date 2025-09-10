using UnityEngine;
using System.Collections;

public class AvengerSoulObserver : MonoBehaviour
{
    public GameObject[] spiritPrefabs;
    public float delayAfterHit = 1f;

    private PlayerHealth playerHealth;
    private float lastKnownHealth;

    private void Start()
    {
        playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
            lastKnownHealth = playerHealth.currentHealth;
    }

    private void Update()
    {
        if (playerHealth == null) return;

        if (playerHealth.currentHealth < lastKnownHealth)
        {
            lastKnownHealth = playerHealth.currentHealth;
            StartCoroutine(SummonSpiritsWithDelay());
        }
        else
        {
            lastKnownHealth = playerHealth.currentHealth;
        }
    }

    private IEnumerator SummonSpiritsWithDelay()
    {
        yield return new WaitForSeconds(delayAfterHit);

        Vector3 basePos = playerHealth.transform.position;

        for (int i = 0; i < spiritPrefabs.Length; i++)
        {
            if (spiritPrefabs[i] == null) continue;

            Vector3 offset = new Vector3((i == 0 ? -0.5f : 0.5f), 0f, 0f);
            Instantiate(spiritPrefabs[i], basePos + offset, Quaternion.identity);
        }
    }
}
