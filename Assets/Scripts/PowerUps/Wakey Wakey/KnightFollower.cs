using UnityEngine;
using System.Collections.Generic;

public class KnightFollower : MonoBehaviour
{
    public float moveSpeed = 90f;
    public float delaySeconds = 2f;
    public float followThreshold = 0.1f;

    private Transform player;
    private Queue<Vector2> positionHistory = new();
    private float timer;

    private void Start()
    {
        // Buscar al player en escena por tag
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null)
            player = found.transform;
        else
            Debug.LogWarning("[KnightFollower] No se encontró ningún jugador con tag 'Player'");
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;

        // Guardar posición actual del jugador una vez por frame
        positionHistory.Enqueue(player.position);

        // Esperar hasta acumular suficiente delay
        if (timer < delaySeconds) return;

        if (positionHistory.Count > Mathf.RoundToInt(delaySeconds / Time.deltaTime))
        {
            // Tomar la posición antigua y mover al knight
            Vector2 targetPos = positionHistory.Dequeue();
            float distance = Vector2.Distance(transform.position, targetPos);

            if (distance > followThreshold)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
        }
    }
}
