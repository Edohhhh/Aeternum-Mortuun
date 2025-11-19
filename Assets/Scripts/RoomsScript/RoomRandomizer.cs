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

public enum Difficulty { Easy, Medium, Hard }

public class RoomRandomizer : MonoBehaviour
{
    public static RoomRandomizer Instance { get; private set; }

    [Header("Sala inicial fija")]
    public RoomData startRoom;

    [Header("Sala del Boss Final")]
    [Tooltip("La sala del boss final. Se inserta justo antes del Victory.")]
    public RoomData bossRoom;

    [Header("Sala de Victoria")]
    public RoomData winRoom;

    [Header("Listas de escenas por dificultad")]
    public List<RoomData> easyRooms = new List<RoomData>();
    public List<RoomData> mediumRooms = new List<RoomData>();
    public List<RoomData> hardRooms = new List<RoomData>();

    [Header("Salas intermedias (1 cada N salas)")]
    public List<RoomData> intermediateRooms = new List<RoomData>();
    public int roomsPerIntermediate = 3;

    [Header("Configuraci?n de la run")]
    public int easyCount = 2;
    public int mediumCount = 2;
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

        // --- 1) Salas normales (easy/medium/hard) ---
        List<RoomData> coreRooms = new List<RoomData>();
        AddRandomRooms(coreRooms, easyRooms, easyCount);
        AddRandomRooms(coreRooms, mediumRooms, mediumCount);
        AddRandomRooms(coreRooms, hardRooms, hardCount);

        // --- 2) Start ---
        if (startRoom != null)
            generatedRun.Add(startRoom);

        // --- 3) Insertar salas intermedias ---
        List<RoomData> shuffledIntermediate = intermediateRooms
            .OrderBy(x => Random.value)
            .ToList();

        int interIndex = 0;
        int roomsSinceLastIntermediate = 0;

        for (int i = 0; i < coreRooms.Count; i++)
        {
            generatedRun.Add(coreRooms[i]);
            roomsSinceLastIntermediate++;

            if (roomsPerIntermediate > 0 &&
                roomsSinceLastIntermediate >= roomsPerIntermediate &&
                interIndex < shuffledIntermediate.Count)
            {
                generatedRun.Add(shuffledIntermediate[interIndex]);
                interIndex++;
                roomsSinceLastIntermediate = 0;
            }
        }

        // --- 4) Boss Final ---
        if (bossRoom != null)
            generatedRun.Add(bossRoom);

        // --- 5) Victory ---
        if (winRoom != null)
            generatedRun.Add(winRoom);

        // --- 6) Filtrar IDs duplicados ---
        HashSet<int> usedIDs = new HashSet<int>();
        generatedRun = generatedRun
            .Where(r => usedIDs.Add(r.id))
            .ToList();

        Debug.Log("[RoomRandomizer] Run generada con " + generatedRun.Count + " salas.");
    }

    private void AddRandomRooms(List<RoomData> target, List<RoomData> sourceList, int count)
    {
        if (sourceList == null || sourceList.Count == 0) return;

        List<RoomData> shuffled = sourceList.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < count && i < shuffled.Count; i++)
            target.Add(shuffled[i]);
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
            Debug.Log("[RoomRandomizer] No quedan m?s salas.");
            SceneManager.LoadScene("Victory");
            return null;
        }
    }
}
