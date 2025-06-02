using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene("Spawn");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
