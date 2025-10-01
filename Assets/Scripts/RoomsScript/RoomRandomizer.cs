using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { Easy, Medium, Hard }

public class RoomRandomizer : MonoBehaviour
{
    public static RoomRandomizer Instance { get; private set; }

    [Header("Listas de escenas por dificultad")]
    public List<string> easyRooms = new List<string>();
    public List<string> mediumRooms = new List<string>();
    public List<string> hardRooms = new List<string>();

    [Header("Configuración de la run")]
    [Tooltip("Cantidad de salas fáciles a incluir en la run")]
    public int easyCount = 2;
    [Tooltip("Cantidad de salas medias a incluir en la run")]
    public int mediumCount = 2;
    [Tooltip("Cantidad de salas difíciles a incluir en la run")]
    public int hardCount = 1;

    [Header("Debug - Lista generada (orden final)")]
    public List<string> generatedRun = new List<string>();

    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Mantener entre escenas
    }

    private void Start()
    {
        GenerateRun();
    }

    /// <summary>
    /// Genera una nueva run en base a las cantidades configuradas
    /// </summary>
    public void GenerateRun()
    {
        generatedRun.Clear();
        currentIndex = 0;

        // Agregar salas según cantidades pedidas
        AddRandomRooms(easyRooms, easyCount);
        AddRandomRooms(mediumRooms, mediumCount);
        AddRandomRooms(hardRooms, hardCount);

        Debug.Log("[RoomRandomizer] Run generada con " + generatedRun.Count + " salas.");
    }

    private void AddRandomRooms(List<string> sourceList, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (sourceList.Count == 0) continue;
            string room = sourceList[Random.Range(0, sourceList.Count)];
            generatedRun.Add(room);
        }
    }

    /// <summary>
    /// Devuelve la siguiente sala en orden, o null si ya no quedan
    /// </summary>
    public string GetNextRoom()
    {
        if (currentIndex < generatedRun.Count)
        {
            string room = generatedRun[currentIndex];
            currentIndex++;
            return room;
        }
        else
        {
            Debug.Log("[RoomRandomizer] No quedan más salas en la run.");
            return null;
        }
    }
}
