using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float defaultDuration = 0.6f;
    [SerializeField] private bool fadeInOnSceneStart = true;
    [SerializeField] private string imageTag = "ScreenFaderImage";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        // intentar asignar si no está en el inspector
        TryFindFadeImage();
    }

    private void Start()
    {
        // Arrancar opaco y hacer Fade In inmediatamente (esto soluciona el "no se aclara al Play")
        if (fadeInOnSceneStart && fadeImage != null)
        {
            SetAlpha(1f);
            fadeImage.raycastTarget = true;
            StartCoroutine(Fade(1f, 0f, defaultDuration));
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Cada vez que entra una escena nueva: Fade In
        if (fadeInOnSceneStart)
        {
            TryFindFadeImage(); // reintentar por si la Image es específica de la escena
            if (fadeImage != null)
            {
                StopAllCoroutines();
                SetAlpha(1f);
                fadeImage.raycastTarget = true;
                StartCoroutine(Fade(1f, 0f, defaultDuration));
            }
        }
    }

    private void TryFindFadeImage()
    {
        if (fadeImage != null) return;

        if (!string.IsNullOrEmpty(imageTag))
        {
            var go = GameObject.FindWithTag(imageTag);
            if (go != null)
            {
                var img = go.GetComponent<Image>();
                if (img != null) { fadeImage = img; return; }
            }
        }

        var byName = GameObject.Find("FadeImage");
        if (byName != null)
        {
            var img = byName.GetComponent<Image>();
            if (img != null) { fadeImage = img; return; }
        }

        // heurística: buscar la primera Image que esté estirada full-screen
        var images = GameObject.FindObjectsOfType<Image>();
        foreach (var i in images)
        {
            var rt = i.rectTransform;
            if (rt != null && rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one)
            {
                fadeImage = i;
                return;
            }
        }
    }

    private void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        var c = fadeImage.color; c.a = a; fadeImage.color = c;
    }

    public void FadeAndLoadScene(string sceneName, float duration = -1f)
    {
        if (duration <= 0) duration = defaultDuration;
        StartCoroutine(FadeAndLoadCoroutine(sceneName, duration));
    }

    private IEnumerator FadeAndLoadCoroutine(string sceneName, float duration)
    {
        if (fadeImage == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f, duration));
        fadeImage.raycastTarget = true;

        // Carga asincrónica
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
        yield return null;

        // OnSceneLoaded ejecutará el Fade In automáticamente
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null) yield break;
        float elapsed = 0f;
        Color c = fadeImage.color;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;
            yield return null;
        }
        c.a = to;
        fadeImage.color = c;

        if (to == 0f)
            fadeImage.raycastTarget = false;
    }
}

