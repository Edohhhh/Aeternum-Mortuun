using UnityEngine;

public class HealthDataNashe : MonoBehaviour
{
    public static HealthDataNashe Instance;

    [Header("Curas persistentes")]
    public int healsLeft = 0;
    public int maxHeals = 3;

    private const string KEY_HEALS = "PLAYER_HEALS";
    private const string KEY_MAXHEALS = "PLAYER_MAXHEALS";

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
    }

    public void AddHeal(int amount)
    {
        healsLeft = Mathf.Clamp(healsLeft + amount, 0, maxHeals);
        SaveData();
    }

    public void SetMaxHeals(int newMax)
    {
        maxHeals = Mathf.Max(1, newMax);
        healsLeft = Mathf.Clamp(healsLeft, 0, maxHeals);
        SaveData();
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt(KEY_HEALS, healsLeft);
        PlayerPrefs.SetInt(KEY_MAXHEALS, maxHeals);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        healsLeft = PlayerPrefs.GetInt(KEY_HEALS, healsLeft);
        maxHeals = PlayerPrefs.GetInt(KEY_MAXHEALS, maxHeals);
    }
}
