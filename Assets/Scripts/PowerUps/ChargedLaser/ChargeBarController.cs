using UnityEngine;

public class ChargeBarController : MonoBehaviour
{
    [Header("Configuración Visual")]
    public float barWidth = 2f;
    public float barHeight = 0.2f;
    public Color lowChargeColor = Color.red;
    public Color mediumChargeColor = Color.yellow;
    public Color fullChargeColor = Color.green;

    private SpriteRenderer fillRenderer;
    private SpriteRenderer backgroundRenderer;
    private Transform fillTransform;
    private bool initialized = false;

    public void Initialize(SpriteRenderer fill, SpriteRenderer background)
    {
        fillRenderer = fill;
        backgroundRenderer = background;
        fillTransform = fill.transform;
        initialized = true;

        SetupBarVisuals();
    }

    private void SetupBarVisuals()
    {
        if (!initialized) return;

        // Configurar fondo
        if (backgroundRenderer != null)
        {
            CreateBarSprite(backgroundRenderer, Color.black, barWidth, barHeight);
        }

        // Configurar relleno
        if (fillRenderer != null)
        {
            CreateBarSprite(fillRenderer, lowChargeColor, barWidth, barHeight);
            fillTransform.localScale = new Vector3(0, 1, 1); // Inicialmente vacío
        }
    }

    private void CreateBarSprite(SpriteRenderer renderer, Color color, float width, float height)
    {
        // Crear una textura simple para la barra
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(width, height);
    }

    public void UpdateCharge(float chargePercentage)
    {
        if (!initialized) return;

        // Actualizar escala del relleno
        if (fillTransform != null)
        {
            fillTransform.localScale = new Vector3(chargePercentage, 1, 1);
        }

        // Actualizar color según la carga
        if (fillRenderer != null)
        {
            Color chargeColor = GetChargeColor(chargePercentage);
            fillRenderer.color = chargeColor;
        }

        // Hacer visible/invisible según la carga
        if (chargePercentage > 0.01f)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    private Color GetChargeColor(float chargePercentage)
    {
        if (chargePercentage >= 1f)
            return fullChargeColor;
        else if (chargePercentage >= 0.5f)
            return mediumChargeColor;
        else
            return lowChargeColor;
    }
}