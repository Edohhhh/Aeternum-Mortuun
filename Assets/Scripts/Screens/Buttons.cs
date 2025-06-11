using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{

    public void PlayAgain()
    {
        SceneManager.LoadScene("BossSlime");
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}
