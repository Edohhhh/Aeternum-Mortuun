using UnityEngine;
using UnityEngine.UI;

public class StarBlink : MonoBehaviour
{
    private Image image;
    private float speed;
    private float baseAlpha;

    void Start()
    {
        image = GetComponent<Image>();
        speed = Random.Range(0.3f, 0.9f);       // Cada estrella titila distinto
        baseAlpha = Random.Range(0.0f, 0.3f);   // Nivel base de transparencia
    }

    void Update()
    {
        float alpha = baseAlpha + Mathf.PingPong(Time.time * speed, 1f - baseAlpha);
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
