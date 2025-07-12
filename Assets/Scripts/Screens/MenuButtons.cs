using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene("BossSlime Nuevo");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
