using UnityEngine;

public class UIParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public RectTransform target;
        public Vector2 strength = new Vector2(10f, 10f);
        private Vector2 initialPosition;

        public void StoreInitialPosition()
        {
            if (target != null)
                initialPosition = target.anchoredPosition;
        }

        public void UpdateParallax(Vector2 input)
        {
            if (target != null)
            {
                target.anchoredPosition = initialPosition + new Vector2(input.x * strength.x, input.y * strength.y);
            }
        }
    }

    public ParallaxLayer[] layers;

    private void Start()
    {
        foreach (var layer in layers)
            layer.StoreInitialPosition();
    }

    private void Update()
    {
        Vector2 mouseNormalized = new Vector2(
            (Input.mousePosition.x / Screen.width - 0.5f) * 2f,
            (Input.mousePosition.y / Screen.height - 0.5f) * 2f
        );

        foreach (var layer in layers)
            layer.UpdateParallax(mouseNormalized);
    }
}