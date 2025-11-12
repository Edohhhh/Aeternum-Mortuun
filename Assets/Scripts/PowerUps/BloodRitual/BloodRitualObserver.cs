using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class BloodRitualObserver : MonoBehaviour
{
    [HideInInspector] public float healChance = 0.1f;
    [HideInInspector] public float healAmount = 0.5f;

    private PlayerController player;
    private readonly HashSet<int> hooked = new(); // instanceID de EnemyHealth ya hookeados
    private Coroutine attachLoop;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        // arranca un loop liviano que ata hooks a enemigos nuevos
        attachLoop = StartCoroutine(AttachLoop());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (attachLoop != null) StopCoroutine(attachLoop);
        hooked.Clear();
    }

    public void BindPlayer(PlayerController pc)
    {
        player = pc;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // re-enlazar player en cada escena
        player = Object.FindFirstObjectByType<PlayerController>();
        // limpiamos el set (enemies de escena anterior ya no valen)
        hooked.Clear();
    }

    private IEnumerator AttachLoop()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            // Encontrar enemigos y enganchar hook si falta
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            for (int i = 0; i < enemies.Length; i++)
            {
                var go = enemies[i];
                if (go == null) continue;

                if (go.TryGetComponent(out EnemyHealth eh))
                {
                    int id = eh.GetInstanceID();
                    if (!hooked.Contains(id))
                    {
                        var hook = go.GetComponent<BloodRitualHook>();
                        if (hook == null) hook = go.AddComponent<BloodRitualHook>();

                        hook.Initialize(eh, healChance, healAmount);
                        hook.SetPlayerRef(player); // puede ser null aquí; el hook se re-vincula si cambia la escena

                        hooked.Add(id);
                    }
                }
            }
            yield return wait;
        }
    }
}
