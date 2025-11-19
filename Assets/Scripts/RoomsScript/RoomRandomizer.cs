using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class RoomData
{
    public int id;
    public string roomName;
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class RoomRandomizer : MonoBehaviour
{
    public static RoomRandomizer Instance { get; private set; }

    [Header("Sala inicial fija")]
    public RoomData startRoom;

    [Header("Sala ante-última fija (combate final previo)")]
    public RoomData finalCombatRoom;

    [Header("Sala de victoria")]
    public RoomData winRoom;

    [Header("Listas de escenas por dificultad")]
    public List<RoomData> easyRooms = new List<RoomData>();
    public List<RoomData> intermediateRooms = new List<RoomData>();  // NUEVO
    public List<RoomData> mediumRooms = new List<RoomData>();
    public List<RoomData> hardRooms = new List<RoomData>();

    [Header("Configuración de la run")]
    public int easyCount = 2;
    public int intermediateCount = 1;  // NUEVO
    public int mediumCount = 2;
    public int intermediate2Count = 1; // NUEVO (segunda intermedia)
    public int hardCount = 1;

    [Header("Debug - Lista generada (orden final)")]
    public List<RoomData> generatedRun = new List<RoomData>();

    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GenerateRun();
    }

    public void GenerateRun()
    {
        generatedRun.Clear();
        currentIndex = 0;

        // 1. Sala inicial
        if (startRoom != null) generatedRun.Add(startRoom);

        // 2. Easy
        AddRandomRooms(easyRooms, easyCount);

        // 3. Intermedio 1
        AddRandomRooms(intermediateRooms, intermediateCount);

        // 4. Medium
        AddRandomRooms(mediumRooms, mediumCount);

        // 5. Intermedio 2
        AddRandomRooms(intermediateRooms, intermediate2Count);

        // 6. Hard
        AddRandomRooms(hardRooms, hardCount);

        // 7. Ante-última (combate final)
        if (finalCombatRoom != null) generatedRun.Add(finalCombatRoom);

        // 8. Sala final (ganar)
        if (winRoom != null) generatedRun.Add(winRoom);

        // Evitar duplicados por ID, respetando orden
        HashSet<int> usedIDs = new HashSet<int>();
        generatedRun = generatedRun
            .Where(r => usedIDs.Add(r.id))
            .ToList();

        Debug.Log("[RoomRandomizer] Run generada con " + generatedRun.Count + " salas.");
    }

    private void AddRandomRooms(List<RoomData> sourceList, int count)
    {
        if (sourceList.Count == 0) return;

        List<RoomData> shuffled = sourceList.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < count && i < shuffled.Count; i++)
        {
            generatedRun.Add(shuffled[i]);
        }
    }

    public string GetNextRoom()
    {
        if (currentIndex < generatedRun.Count)
        {
            string room = generatedRun[currentIndex].roomName;
            currentIndex++;
            return room;
        }
        else
        {
            Debug.Log("[RoomRandomizer] No quedan más salas en la run.");
            SceneManager.LoadScene("Victory");
            return null;
        }
    }
}
