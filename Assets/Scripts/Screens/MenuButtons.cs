using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene("LoopSlime1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
