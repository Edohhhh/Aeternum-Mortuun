using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Tooltip("Nombre de la escena a cargar al pulsar la flecha derecha")]
    public string nextSceneName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 1) Guardar stats actuales:
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player != null)
                player.SavePlayerData();

            // 2) Cargar la siguiente escena:
            SceneManager.LoadScene("SampleScene 1");
        }
    }
}