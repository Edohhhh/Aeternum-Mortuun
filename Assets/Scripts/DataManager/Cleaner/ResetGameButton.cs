using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResetGameButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonPressed);
    }

    public void OnButtonPressed()
    {
        var player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            // 🔹 Si hay player en escena (ejemplo: gameplay)
            GameDataManager.Instance.ResetPlayerCompletely(player);
        }
        else
        {
            // 🔹 Si NO hay player (ejemplo: Win/Lose)
            GameDataManager.Instance.ResetAllWithoutPlayer();
        }

        Debug.Log("[ResetGameButton] Reset ejecutado.");
    }
}