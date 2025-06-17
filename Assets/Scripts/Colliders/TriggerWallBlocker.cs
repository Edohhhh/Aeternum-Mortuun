using UnityEngine;

public class TriggerWallBlocker : MonoBehaviour
{
    private bool isBlocked = false;
    private Vector2 lastSafePosition;

    private void Update()
    {
        if (!isBlocked)
        {
            // Guardamos la última posición válida
            lastSafePosition = transform.position;
        }
        else
        {
            
            transform.position = lastSafePosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            isBlocked = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            isBlocked = false;
        }
    }
}