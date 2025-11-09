using UnityEngine;

public class SpiderFollowY : MonoBehaviour
{
    public Transform player;  // arrastrá aquí el jugador
    public float speed = 2f;  // velocidad del seguimiento vertical

    void Update()
    {
        if (player == null) return;

        // Mover solo en Y hacia la altura del jugador
        float newY = Mathf.MoveTowards(transform.position.y, player.position.y, speed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
