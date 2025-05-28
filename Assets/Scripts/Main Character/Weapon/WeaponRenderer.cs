using UnityEngine;

public class WeaponRenderer : MonoBehaviour
{
    public Transform weaponTransform;         // Sprite del arma
    public SpriteRenderer weaponSprite;       // SpriteRenderer del arma
    public PlayerAttack playerAttack;         // Referencia al script de ataque

    public Transform swordOrigin;             // El transform llamado "Mango"
    public float swordDistance = 1f;          // Qué tan lejos se posiciona desde el jugador
    public float showDelay = 0.08f;
    public float hideAfter = 0.25f;

    void LateUpdate()
    {
        // Mostrar espada solo en el momento justo
        if (playerAttack.animTime > showDelay && playerAttack.animTime < hideAfter)
        {
            weaponSprite.enabled = true;

            // Calcular dirección al mouse
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 dir = (mouseWorld - playerAttack.transform.position).normalized;

            // Mover y rotar el punto de origen (Mango)
            if (swordOrigin != null)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                swordOrigin.rotation = Quaternion.Euler(0f, 0f, angle);
                swordOrigin.position = playerAttack.transform.position + (Vector3)(dir * swordDistance);
            }

            // Posicionar y rotar el arma igual que el Mango
            if (weaponTransform != null && swordOrigin != null)
            {
                weaponTransform.position = swordOrigin.position;
                weaponTransform.rotation = swordOrigin.rotation;
                weaponSprite.flipY = dir.x < 0;
            }
        }
        else
        {
            weaponSprite.enabled = false;
        }
    }
}
