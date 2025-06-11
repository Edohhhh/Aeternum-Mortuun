using EasyUI.PickerWheelUI;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
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
        Debug.Log("Enemy registered.");
    }

    public void UnregisterEnemy()
    {
        Debug.Log("Enemy unregistered.");

        // Esperar un frame para asegurar que el objeto fue destruido
        Instance.StartCoroutine(CheckEnemiesNextFrame());
    }

    private System.Collections.IEnumerator CheckEnemiesNextFrame()
    {
        yield return new WaitForEndOfFrame();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            Debug.Log("No enemies left. Showing wheel UI.");
            WheelUIController uiController = FindFirstObjectByType<WheelUIController>();
            if (uiController != null)
                uiController.MostrarRuleta();
            else
                Debug.LogWarning("WheelUIController not found.");
        }
        else
        {
            Debug.Log($"Enemies remaining: {enemies.Length}");
        }
    }
}
//using EasyUI.PickerWheelUI;
//using UnityEngine;

//public class EnemyManager : MonoBehaviour
//{
//    [SerializeField] private int enemyCount = 0;

//    public static EnemyManager Instance;

//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }

//    public void RegisterEnemy()
//    {
//        enemyCount++;
//        Debug.Log($"Enemy registered. Count = {enemyCount}");
//    }

//    public void UnregisterEnemy()
//    {
//        enemyCount = Mathf.Max(0, enemyCount - 1);
//        Debug.Log($"Enemy unregistered. Count = {enemyCount}");

//        if (enemyCount == 0)
//        {
//            WheelUIController uiController = FindFirstObjectByType<WheelUIController>();
//            if (uiController != null)
//            {
//                Debug.Log("No enemies left. Showing wheel UI.");
//                uiController.MostrarRuleta();
//            }
//            else
//            {
//                Debug.LogWarning("WheelUIController not found when trying to show ruleta.");
//            }
//        }
//    }

//    public int GetEnemyCount() => enemyCount;
//}