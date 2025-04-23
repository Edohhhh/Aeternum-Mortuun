using System.Collections;
using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    public GameObject attackPrefab;
    public float attackDuration = 1f;
    public bool canAttack = true;

    private GameObject currentAttack;

    void Update()
    {
        // Solo permitir atacar si no estamos en un estado de dash
        if (canAttack)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) Attack(Vector2.up);
            else if (Input.GetKeyDown(KeyCode.DownArrow)) Attack(Vector2.down);
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) Attack(Vector2.left);
            else if (Input.GetKeyDown(KeyCode.RightArrow)) Attack(Vector2.right);
        }
    }

    void Attack(Vector2 direction)
    {
        if (attackPrefab == null) return;

        // Si ya existe un ataque, destr�yelo antes de instanciar uno nuevo
        if (currentAttack != null)
            Destroy(currentAttack);

        // Instanciamos el ataque en la posici�n del jugador
        currentAttack = Instantiate(attackPrefab, transform.position + (Vector3)direction, Quaternion.identity);
        currentAttack.transform.parent = transform; // Sigue al jugador

        // Rotar el ataque dependiendo de la direcci�n
        RotateAttack(direction);

        canAttack = false; // Bloqueamos el ataque mientras lo estamos ejecutando

        StartCoroutine(DestroyAttackAfterTime());
    }

    // Funci�n para rotar el objeto de ataque
    void RotateAttack(Vector2 direction)
    {
        float angle = 0f;

        // Determinar el �ngulo en funci�n de la direcci�n
        if (direction == Vector2.up)
            angle = 0f;
        else if (direction == Vector2.down)
            angle = 180f;
        else if (direction == Vector2.left)
            angle = 90f;
        else if (direction == Vector2.right)
            angle = -90f;

        // Aplicar la rotaci�n al objeto de ataque
        currentAttack.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    IEnumerator DestroyAttackAfterTime()
    {
        yield return new WaitForSeconds(attackDuration);
        if (currentAttack != null)
        {
            Destroy(currentAttack);
        }

        canAttack = true; // Reactivamos el ataque despu�s de la duraci�n
    }
}
