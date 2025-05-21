using EasyUI.PickerWheelUI;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private int enemyCount = 0;

    public static EnemyManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterEnemy()
    {
        enemyCount++;
        Debug.Log($"Enemy registered. Count = {enemyCount}");
    }

    public void UnregisterEnemy()
    {
        enemyCount = Mathf.Max(0, enemyCount - 1);
        Debug.Log($"Enemy unregistered. Count = {enemyCount}");

        if (enemyCount == 0)
        {
            WheelUIController uiController = FindFirstObjectByType<WheelUIController>();
            if (uiController != null)
            {
                Debug.Log("No enemies left. Showing wheel UI.");
                uiController.MostrarRuleta();
            }
            else
            {
                Debug.LogWarning("WheelUIController not found when trying to show ruleta.");
            }
        }
    }

    public int GetEnemyCount() => enemyCount;
}
