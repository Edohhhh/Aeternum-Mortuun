using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDecay = 40f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private PlayerMeleeAttack meleeAttack; // Referencia al script de ataque

    private Vector2 direction;
    private Vector2 dashDirection;
    private float currentSlideSpeed;
    private bool isSliding = false;
    private bool canDash = true;

    private Rigidbody2D rb2D;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Solo permitir movimiento si no estamos haciendo un dash
        if (!isSliding)
        {
            direction = Vector2.zero;

            if (Input.GetKey(KeyCode.W)) direction.y += 1;
            if (Input.GetKey(KeyCode.S)) direction.y -= 1;
            if (Input.GetKey(KeyCode.D)) direction.x += 1;
            if (Input.GetKey(KeyCode.A)) direction.x -= 1;

            direction = direction.normalized;
        }

        // Solo permitir dash si no estamos atacando y si no estamos dashing ya
        if (meleeAttack.canAttack && !isSliding && Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            StartDash();
        }
    }

    private void FixedUpdate()
    {
        if (isSliding)
        {
            HandleSlide();
        }
        else
        {
            rb2D.MovePosition(rb2D.position + direction * movementSpeed * Time.fixedDeltaTime);
        }
    }

    private void StartDash()
    {
        if (!canDash) return;

        isSliding = true;
        canDash = false;
        currentSlideSpeed = dashSpeed;
        dashDirection = direction == Vector2.zero ? Vector2.right : direction;
        meleeAttack.canAttack = false; // Bloqueamos el ataque mientras estamos dashing
        StartCoroutine(RotateDuringDash());
    }

    private void HandleSlide()
    {
        rb2D.MovePosition(rb2D.position + dashDirection * currentSlideSpeed * Time.fixedDeltaTime);
        currentSlideSpeed -= dashDecay * Time.fixedDeltaTime;

        if (currentSlideSpeed <= 0.1f)
        {
            isSliding = false;
            meleeAttack.canAttack = true; // Reactivamos el ataque
            Invoke(nameof(ResetDash), dashCooldown);
        }
    }

    private void ResetDash()
    {
        canDash = true;
    }

    private System.Collections.IEnumerator RotateDuringDash()
    {
        float totalRotation = 0f;
        float rotationSpeed = 720f; // grados por segundo

        while (isSliding && totalRotation < 360f)
        {
            float rotationStep = rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, rotationStep);
            totalRotation += rotationStep;
            yield return null;
        }

        transform.rotation = Quaternion.identity; // Reiniciamos rotación por si se pasa
    }
}
