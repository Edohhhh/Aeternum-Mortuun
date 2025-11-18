//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections;

//public class RockHardObserver : MonoBehaviour
//{
//    // Config (seteada desde el SO)
//    [HideInInspector] public RockHardDamageMode damageMode;
//    [HideInInspector] public float flatDamage;
//    [HideInInspector] public float expFactor;
//    [HideInInspector] public int expSteps;
//    [HideInInspector] public float moveSpeedDelta;
//    [HideInInspector] public float dashCooldownMultiplier;

//    private PlayerController player;
//    private RockHardApplier currentApplier;

//    private void Awake()
//    {
//        DontDestroyOnLoad(gameObject);
//        SceneManager.sceneLoaded += OnSceneLoaded;
//    }

//    private void OnDestroy()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//        // limpiezas finales
//        if (currentApplier != null) Destroy(currentApplier);
//    }

//    public void AttachToPlayer(PlayerController pc)
//    {
//        player = pc;
//        ApplyNow();
//    }

//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        StartCoroutine(ReassignPlayer());
//    }
//    private IEnumerator ReassignPlayer()
//    {
//        // Esperar player real
//        PlayerController found = null;
//        while (found == null)
//        {
//            found = Object.FindFirstObjectByType<PlayerController>();
//            yield return null;
//        }

//        // Esperar FULL LOAD PLAYER DATA
//        yield return new WaitForEndOfFrame();
//        yield return new WaitForEndOfFrame();

//        // Reaplicar buff SIEMPRE
//        player = found;
//        ApplyNow();
//    }

//    private void ApplyNow()
//    {
//        if (player == null) return;

//        // Si ya había un applier, limpiarlo y volver a aplicar para el nuevo player
//        if (currentApplier != null) Destroy(currentApplier);

//        currentApplier = player.gameObject.AddComponent<RockHardApplier>();
//        currentApplier.Apply(
//            damageMode, flatDamage, expFactor, expSteps,
//            moveSpeedDelta, dashCooldownMultiplier
//        );
//    }
//}
