using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneEnder : MonoBehaviour
{
    public PlayableDirector director;
    public string nextSceneName = "NombreDeLaEscena";

    void Start()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        director.stopped += OnTimelineFinished;
    }

    void OnTimelineFinished(PlayableDirector d)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
