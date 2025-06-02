using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnCode : MonoBehaviour
{
    [Tooltip("Player")]
    public Transform destination;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {

            SceneManager.LoadScene("BossSlime");

        }
    }
}
