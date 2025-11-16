using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class SpawnEntry
{
    public GameObject prefab;
    public Transform waypoint;
    public float interval = 1f;
    public int count = 1;
    public bool enabled = true;
}

[System.Serializable]
public class Wave
{
    public string name = "Wave";
    public List<SpawnEntry> entries = new List<SpawnEntry>();
}

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();
    public int wavesToComplete = 0; // si 0 => usar waves.Count

    [Header("Behavior")]
    public bool startOnAwake = false;
    public GameObject destroyTargetOnWavesComplete;

    [Header("Parallel timer (opcional)")]
    public bool startParallelTimerOnStart = false;
    public float parallelTimerDuration = 30f;
    public GameObject parallelTimerDestroyTarget;

    [Header("Parallel timer UI")]
    public TextMeshProUGUI parallelTimerText;

    // ★ NUEVO: UI para waves completadas
    [Header("Waves UI")]
    public TextMeshProUGUI wavesCompletedText;

    [Header("Events")]
    public UnityEvent onWavesComplete;

    // internal
    private int _currentWaveIndex = -1;
    private bool _running = false;

    private int _aliveEnemies = 0;
    private int _pendingSpawns = 0;

    // ★ NUEVO: contador interno
    private int _wavesCompleted = 0;

    private void Start()
    {
        // ★ Inicializar UI al inicio para que muestre "0 / total" aunque no se haya iniciado StartWaves().
        UpdateWavesUI();

        if (startOnAwake)
            StartWaves();
    }

    // ★ Helper para actualizar la UI desde cualquier lugar (evita duplicación)
    private void UpdateWavesUI()
    {
        if (wavesCompletedText == null) return;

        int total = (wavesToComplete > 0) ? wavesToComplete : waves.Count;
        // Asegurarse que _wavesCompleted no sea mayor que total (por seguridad).
        int clamped = Mathf.Clamp(_wavesCompleted, 0, Mathf.Max(0, total));
        wavesCompletedText.text = clamped + " / " + total;
    }

    public void StartWaves()
    {
        if (_running) return;
        if (waves == null || waves.Count == 0)
        {
            Debug.LogWarning("[WaveManager] No waves configured.", this);
            return;
        }

        _running = true;
        _currentWaveIndex = -1;

        // ★ Reiniciar contador de waves completadas cuando se inicia el sistema.
        _wavesCompleted = 0;
        UpdateWavesUI(); // ★ actualizar UI inmediatamente a "0 / total"

        if (startParallelTimerOnStart)
            StartCoroutine(ParallelTimerRoutine());

        StartCoroutine(NextWaveRoutine());
        Debug.Log("[WaveManager] Waves started.", this);
    }

    private IEnumerator NextWaveRoutine()
    {
        int targetWaves = (wavesToComplete > 0) ? wavesToComplete : waves.Count;

        while (_running)
        {
            _currentWaveIndex++;
            if (_currentWaveIndex >= waves.Count)
            {
                Debug.Log("[WaveManager] No more configured waves.", this);
                break;
            }

            Debug.Log("[WaveManager] Starting wave " +
                (_currentWaveIndex + 1) + " / " + waves.Count +
                " (" + waves[_currentWaveIndex].name + ")", this);

            yield return StartCoroutine(RunWave(waves[_currentWaveIndex]));

            // ★ incrementar contador y actualizar UI cuando termina una wave
            _wavesCompleted++;

            UpdateWavesUI();
            // ★ FIN NUEVO -----------------

            if ((_currentWaveIndex + 1) >= targetWaves)
            {
                Debug.Log("[WaveManager] Reached target waves: " + targetWaves, this);

                if (destroyTargetOnWavesComplete != null)
                {
                    Debug.Log("[WaveManager] Destroying target on waves complete: " +
                        destroyTargetOnWavesComplete.name, this);
                    Destroy(destroyTargetOnWavesComplete);
                }

                yield return null;

                if (onWavesComplete != null)
                {
                    try
                    {
                        onWavesComplete.Invoke();
                        Debug.Log("[WaveManager] onWavesComplete invoked.", this);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning(
                            "[WaveManager] Exception invoking onWavesComplete: " + ex.Message, this);
                    }
                }

                _running = false;
                yield break;
            }

            yield return new WaitForSeconds(1f);
        }

        _running = false;
    }

    private IEnumerator RunWave(Wave wave)
    {
        if (wave == null)
        {
            Debug.LogWarning("[WaveManager] RunWave called with null wave.", this);
            yield break;
        }

        _aliveEnemies = 0;
        _pendingSpawns = 0;

        for (int i = 0; i < wave.entries.Count; i++)
        {
            SpawnEntry entry = wave.entries[i];
            if (entry != null && entry.enabled && entry.prefab != null && entry.interval >= 0f && entry.count > 0)
            {
                _pendingSpawns += entry.count;
                StartCoroutine(SpawnLoop(entry));
            }
            else
            {
                string info = "Entry " + i + " skipped:";
                if (entry == null) info += " entry==null";
                else
                {
                    info += " enabled=" + entry.enabled;
                    info += " prefab=" + (entry.prefab != null ? entry.prefab.name : "null");
                    info += " interval=" + entry.interval;
                    info += " count=" + entry.count;
                }
                Debug.Log("[WaveManager] " + info, this);
            }
        }

        while ((_pendingSpawns > 0) || (_aliveEnemies > 0))
        {
            yield return null;
        }

        Debug.Log("[WaveManager] Wave '" + wave.name + "' completed.", this);
    }

    private IEnumerator SpawnLoop(SpawnEntry entry)
    {
        if (entry == null) yield break;

        int remaining = entry.count;
        Debug.Log("[WaveManager] SpawnLoop starting for prefab " +
            (entry.prefab != null ? entry.prefab.name : "null") +
            " count=" + remaining + " interval=" + entry.interval, this);

        while (remaining > 0)
        {
            if (entry.prefab == null)
            {
                Debug.LogWarning("[WaveManager] SpawnLoop: prefab is null, skipping remaining.", this);
                _pendingSpawns -= remaining;
                if (_pendingSpawns < 0) _pendingSpawns = 0;
                yield break;
            }

            Vector3 spawnPos = (entry.waypoint != null) ? entry.waypoint.position : this.transform.position;
            GameObject go = Instantiate(entry.prefab, spawnPos, Quaternion.identity);

            SpawnedEnemy tracker = go.AddComponent<SpawnedEnemy>();
            tracker.manager = this;

            _aliveEnemies++;

            Debug.Log("[WaveManager] Spawned " + go.name + " at " + spawnPos +
                " (alive now: " + _aliveEnemies + ")", this);

            remaining--;

            if (remaining > 0)
                yield return new WaitForSeconds(entry.interval);
        }

        _pendingSpawns -= entry.count;
        if (_pendingSpawns < 0) _pendingSpawns = 0;

        Debug.Log("[WaveManager] SpawnLoop finished for prefab " +
            (entry.prefab != null ? entry.prefab.name : "null"), this);
    }

    public void NotifyEnemyDestroyed()
    {
        _aliveEnemies--;
        if (_aliveEnemies < 0) _aliveEnemies = 0;

        Debug.Log("[WaveManager] Enemy destroyed. Alive left: " + _aliveEnemies, this);
    }

    private IEnumerator ParallelTimerRoutine()
    {
        float elapsed = 0f;
        Debug.Log("[WaveManager] Parallel timer started: " + parallelTimerDuration + "s", this);

        while (elapsed < parallelTimerDuration && _running)
        {
            elapsed += Time.deltaTime;
            float remaining = Mathf.Clamp(parallelTimerDuration - elapsed, 0, parallelTimerDuration);

            if (parallelTimerText != null)
                parallelTimerText.text = Mathf.CeilToInt(remaining).ToString();

            yield return null;
        }

        if (!_running)
        {
            Debug.Log("[WaveManager] Parallel timer stopped because waves finished.", this);
            yield break;
        }

        Debug.Log("[WaveManager] Parallel timer finished.", this);

        if (parallelTimerDestroyTarget != null)
        {
            Debug.Log("[WaveManager] Destroying parallelTimerDestroyTarget: " +
                parallelTimerDestroyTarget.name, this);
            Destroy(parallelTimerDestroyTarget);
        }
    }
}
