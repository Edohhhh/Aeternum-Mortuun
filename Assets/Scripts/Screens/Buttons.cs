using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{

    public void PlayAgain()
    {
        // Asegurarnos de que exista el RoomRandomizer
        if (RoomRandomizer.Instance == null)
        {
            Debug.LogError("[Buttons] No se encontró un RoomRandomizer en la escena inicial.");
            return;
        }

        // Generar la run antes de empezar
        RoomRandomizer.Instance.GenerateRun();

        // Cargar la primera sala de la lista
        string firstScene = RoomRandomizer.Instance.GetNextRoom();

        if (!string.IsNullOrEmpty(firstScene))
        {
            SceneManager.LoadScene(firstScene);
        }
        else
        {
            Debug.LogError("[Buttons] La run generada no tiene salas configuradas.");
        }
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}
