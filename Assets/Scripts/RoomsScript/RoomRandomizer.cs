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
    [Tooltip("Sala que siempre aparecerá al inicio de la run")]
    public RoomData startRoom;

    [Header("Listas de escenas por dificultad")]
    public List<RoomData> easyRooms = new List<RoomData>();
    public List<RoomData> mediumRooms = new List<RoomData>();
    public List<RoomData> hardRooms = new List<RoomData>();

    [Header("Configuración de la run")]
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

        // Si hay una sala fija asignada, agregarla al principio
        if (startRoom != null)
        {
            generatedRun.Add(startRoom);
        }

        // Luego agregar las salas aleatorias
        AddRandomRooms(easyRooms, easyCount);
        AddRandomRooms(mediumRooms, mediumCount);
        AddRandomRooms(hardRooms, hardCount);

        // Evitar IDs duplicados (mantiene el orden)
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
