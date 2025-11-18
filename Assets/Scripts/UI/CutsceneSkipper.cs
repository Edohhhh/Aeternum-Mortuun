using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneSkipper : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName = "NombreDeLaEscena";
    public GameObject skipUI;  // Texto o icono en la esquina inferior

    void Start()
    {
        if (director == null)
            director = FindObjectOfType<PlayableDirector>();

        if (skipUI != null)
            skipUI.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Skip();
        }
    }

    public void Skip()
    {
        director.time = director.duration; // Forzar fin de Timeline
        director.Evaluate();
        SceneManager.LoadScene(nextSceneName);
    }
}
