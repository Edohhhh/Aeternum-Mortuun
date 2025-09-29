using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
using System;

namespace EasyUI.PickerWheelUI
{
    [Serializable]
    public class WeightedPowerUpPool
    {
        public PowerUpPool pool;
        [Range(0f, 100f)] public float weight = 1f; // Porcentaje/ponderación para elegir este pool
    }

    public class PickerWheel : MonoBehaviour
    {
        // ✅ NUEVO: lista de pools ponderados
        [Header("Pools ponderados (elige 1 por porcentaje)")]
        [SerializeField] private List<WeightedPowerUpPool> powerUpPools = new List<WeightedPowerUpPool>();

        // ⚠️ LEGACY: por compatibilidad. Si hay pools en la lista, se ignora este campo.
        [SerializeField, Tooltip("LEGACY: se usa sólo si la lista de pools está vacía")]
        private PowerUpPool powerUpPool;

        [Header("Referencias :")]
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private Transform linesParent;
        [SerializeField] private Transform selector;

        private float ruletaUltimoAngulo = 0f;

        [Space]
        [SerializeField] private Transform PickerWheelTransform;
        [SerializeField] private Transform wheelCircle;
        [SerializeField] private GameObject wheelPiecePrefab;
        [SerializeField] private Transform wheelPiecesParent;

        [Space]
        [Header("Sonidos :")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip tickAudioClip;
        [SerializeField][Range(0f, 1f)] private float volume = .5f;
        [SerializeField][Range(-3f, 3f)] private float pitch = 1f;

        [Space]
        [Header("Configuración :")]
        [Range(1, 20)] public int spinDuration = 8;
        [SerializeField][Range(.2f, 2f)] private float wheelSize = 1f;

        [Space]
        [Header("Premios :")]
        public WheelPiece[] wheelPieces;

        [Header("Usos disponibles")]
        [SerializeField] private int usosMaximos = 3;
        private int usosRestantes;
        public int UsosRestantes => usosRestantes;

        public Action<WheelPiece> OnSpinEnd;

        private UnityAction onSpinStartEvent;
        private UnityAction<WheelPiece> onSpinEndEvent;

        private bool _isSpinning = false;
        public bool IsSpinning => _isSpinning;

        private Vector2 pieceMinSize = new Vector2(81f, 146f);
        private Vector2 pieceMaxSize = new Vector2(144f, 213f);
        private int piecesMin = 2;
        private int piecesMax = 12;

        private float pieceAngle;
        private float halfPieceAngle;
        private float halfPieceAngleWithPaddings;

        private double accumulatedWeight;
        private System.Random rand = new System.Random();
        private List<int> nonZeroChancesIndices = new List<int>();

        private WheelPiece ultimoPremio;

        private void Awake()
        {
            usosRestantes = usosMaximos;
        }

        private void Start()
        {
            // ✅ Carga desde pool ponderado (o legacy)
            CargarPremiosDesdePoolsPonderados();

            pieceAngle = 360f / wheelPieces.Length;
            halfPieceAngle = pieceAngle / 2f;
            halfPieceAngleWithPaddings = halfPieceAngle - (halfPieceAngle / 4f);

            Generate();
            CalculateWeightsAndIndices();

            if (nonZeroChancesIndices.Count == 0)
                Debug.LogError("You can't set all pieces chance to zero");

            SetupAudio();
        }

        private void SetupAudio()
        {
            audioSource.clip = tickAudioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
        }
        public void CargarPremiosDesdePool()
        {
            CargarPremiosDesdePoolsPonderados();
        }
        public void Generate()
        {
            GameObject tempPiece = InstantiatePiece();
            RectTransform rt = tempPiece.transform.GetChild(0).GetComponent<RectTransform>();

            float pieceWidth = Mathf.Lerp(pieceMinSize.x, pieceMaxSize.x, 1f - Mathf.InverseLerp(piecesMin, piecesMax, wheelPieces.Length));
            float pieceHeight = Mathf.Lerp(pieceMinSize.y, pieceMaxSize.y, 1f - Mathf.InverseLerp(piecesMin, piecesMax, wheelPieces.Length));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pieceWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pieceHeight);

            Destroy(tempPiece);

            for (int i = 0; i < wheelPieces.Length; i++)
                DrawPiece(i);
        }

        private void DrawPiece(int index)
        {
            WheelPiece piece = wheelPieces[index];
            Transform pieceTrns = InstantiatePiece().transform.GetChild(0);
            pieceTrns.GetChild(0).GetComponent<Image>().sprite = piece.Icon;
            pieceTrns.GetChild(1).GetComponent<Text>().text = piece.Label;
            pieceTrns.GetChild(2).GetComponent<Text>().text = piece.Amount.ToString();

            Transform lineTrns = Instantiate(linePrefab, linesParent.position, Quaternion.identity, linesParent).transform;
            lineTrns.RotateAround(wheelPiecesParent.position, Vector3.back, (pieceAngle * index) + halfPieceAngle);
            pieceTrns.RotateAround(wheelPiecesParent.position, Vector3.back, pieceAngle * index);
        }

        private GameObject InstantiatePiece()
        {
            return Instantiate(wheelPiecePrefab, wheelPiecesParent);
        }

        public void Spin()
        {
            if (_isSpinning) return;

            if (usosRestantes <= 0)
            {
                Debug.LogWarning($"{gameObject.name} no tiene más usos.");
                return;
            }

            _isSpinning = true;
            onSpinStartEvent?.Invoke();

            int index = GetRandomPieceIndex();
            WheelPiece piece = wheelPieces[index];

            float angle = pieceAngle * index;
            float randomOffset = UnityEngine.Random.Range(-halfPieceAngleWithPaddings, halfPieceAngleWithPaddings);
            float finalAngle = angle + randomOffset;
            float totalRotation = finalAngle + 360f * spinDuration;
            Vector3 targetRotation = Vector3.forward * totalRotation;

            float prevAngle = wheelCircle.eulerAngles.z;
            float currentAngle = prevAngle;
            bool isIndicatorOnLine = false;

            wheelCircle
                .DORotate(targetRotation, spinDuration, RotateMode.FastBeyond360)
               .SetEase(Ease.OutQuad) // arranque más suave, frenada progresiva
               .SetUpdate(true)
                .OnUpdate(() =>
                {
                    float diff = Mathf.Abs(prevAngle - currentAngle);
                    if (diff >= halfPieceAngle)
                    {
                        if (isIndicatorOnLine)
                            audioSource.PlayOneShot(audioSource.clip);

                        prevAngle = currentAngle;
                        isIndicatorOnLine = !isIndicatorOnLine;
                    }
                    currentAngle = wheelCircle.eulerAngles.z;
                })
                .OnComplete(() =>
                {
                    _isSpinning = false;
                    usosRestantes--;

                    if (usosRestantes <= 0)
                        Debug.Log($"{gameObject.name} se quedó sin usos.");

                    ruletaUltimoAngulo = wheelCircle.eulerAngles.z;

                    Vector3 selectorPos = selector.position;

                    float minDist = float.MaxValue;
                    int landedIndex = 0;

                    for (int i = 0; i < wheelPiecesParent.childCount; i++)
                    {
                        Transform pieceTransform = wheelPiecesParent.GetChild(i).GetChild(0);
                        float dist = Vector3.Distance(pieceTransform.position, selectorPos);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            landedIndex = i;
                        }
                    }

                    ultimoPremio = wheelPieces[landedIndex];

                    Debug.Log($"🎯 Selector está sobre la pieza {landedIndex}: {ultimoPremio.Label} (distancia {minDist:0.00})");

                    OnSpinEnd?.Invoke(ultimoPremio);
                    onSpinEndEvent?.Invoke(ultimoPremio);
                });
        }

        private int GetRandomPieceIndex()
        {
            double r = rand.NextDouble() * accumulatedWeight;
            for (int i = 0; i < wheelPieces.Length; i++)
                if (r < wheelPieces[i]._weight)
                    return i;
            return 0;
        }

        private void CalculateWeightsAndIndices()
        {
            accumulatedWeight = 0;
            nonZeroChancesIndices.Clear();

            for (int i = 0; i < wheelPieces.Length; i++)
            {
                WheelPiece piece = wheelPieces[i];
                accumulatedWeight += piece.Chance;
                piece._weight = accumulatedWeight;
                piece.Index = i;

                if (piece.Chance > 0)
                    nonZeroChancesIndices.Add(i);
            }
        }

        public void AddSpinStartListener(UnityAction callback) => onSpinStartEvent += callback;
        public void AddSpinEndListener(UnityAction<WheelPiece> callback) => onSpinEndEvent += callback;
        public WheelPiece ObtenerUltimoPremio() => ultimoPremio;

        private void OnValidate()
        {
            if (PickerWheelTransform != null)
                PickerWheelTransform.localScale = new Vector3(wheelSize, wheelSize, 1f);

            if (wheelPieces != null && (wheelPieces.Length > piecesMax || wheelPieces.Length < piecesMin))
                Debug.LogError("[ PickerWheel ] pieces length must be between " + piecesMin + " and " + piecesMax);
        }

        public void AplicarUltimoPremio()
        {
            if (ultimoPremio == null)
            {
                Debug.LogWarning("⚠️ No hay premio para aplicar.");
                return;
            }

            if (ultimoPremio.Effect == null)
            {
                Debug.LogError($"❌ El WheelPiece '{ultimoPremio.Label}' no tiene asignado un PowerUpEffect.");
                return;
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogError("❌ No se encontró el Player en la escena.");
                return;
            }

            ultimoPremio.Effect.Apply(player);
            Debug.Log($"✅ PowerUp aplicado: {ultimoPremio.Effect.label}");
        }

        /// <summary>
        /// ✅ Elige UN pool por porcentaje (weight) y carga sus entradas como piezas de la ruleta.
        /// Si la lista está vacía, usa el campo legacy 'powerUpPool'.
        /// </summary>
        public void CargarPremiosDesdePoolsPonderados()
        {
            PowerUpPool elegido = null;

            if (powerUpPools != null && powerUpPools.Count > 0)
            {
                float total = 0f;
                foreach (var w in powerUpPools)
                {
                    if (w != null && w.pool != null && w.weight > 0f)
                        total += w.weight;
                }

                if (total <= 0f)
                {
                    Debug.LogError("❌ Todos los pesos de los pools están en 0. Asigná weights > 0.");
                    return;
                }

                float r = UnityEngine.Random.Range(0f, total);
                float acc = 0f;

                foreach (var w in powerUpPools)
                {
                    if (w == null || w.pool == null || w.weight <= 0f) continue;
                    acc += w.weight;
                    if (r <= acc)
                    {
                        elegido = w.pool;
                        break;
                    }
                }

                if (elegido == null)
                {
                    // Fallback por si algo raro pasó con la acumulación
                    foreach (var w in powerUpPools)
                    {
                        if (w != null && w.pool != null && w.weight > 0f)
                        {
                            elegido = w.pool;
                            break;
                        }
                    }
                }

                if (elegido != null)
                    Debug.Log($"🎲 Pool elegido por porcentaje: {elegido.name}");
            }
            else
            {
                // Legacy
                elegido = powerUpPool;
                if (elegido != null)
                    Debug.Log($"(LEGACY) Usando PowerUpPool: {elegido.name}");
            }

            if (elegido == null || elegido.entries == null || elegido.entries.Length == 0)
            {
                Debug.LogError("❌ No hay PowerUpPool válido o está vacío.");
                return;
            }

            // Construir piezas desde el pool elegido
            wheelPieces = new WheelPiece[elegido.entries.Length];

            for (int i = 0; i < elegido.entries.Length; i++)
            {
                PowerUpEntry entry = elegido.entries[i];

                if (entry == null || entry.effect == null)
                {
                    Debug.LogError($"❌ Entrada nula o sin efecto en índice {i}");
                    continue;
                }

                if (entry.effect.powerUp == null)
                {
                    Debug.LogWarning($"⚠️ El PowerUpEffect '{entry.effect.label}' no tiene PowerUp asignado.");
                }

                wheelPieces[i] = new WheelPiece
                {
                    Icon = entry.effect.icon,
                    Label = entry.effect.label,
                    Amount = 1,
                    Chance = entry.chance,   // <- Se usan las chances internas del pool elegido
                    Effect = entry.effect
                };

           
            }

            // Actualizar ángulos
            pieceAngle = 360f / wheelPieces.Length;
            halfPieceAngle = pieceAngle / 2f;
            halfPieceAngleWithPaddings = halfPieceAngle - (halfPieceAngle / 4f);

            // Limpiar visual previo
            foreach (Transform child in wheelPiecesParent)
                Destroy(child.gameObject);
            foreach (Transform child in linesParent)
                Destroy(child.gameObject);

            // Redibujar ruleta
            Generate();

            // Recalcular pesos de probabilidad de las piezas
            CalculateWeightsAndIndices();

            // Debug opcional
            RevisarWheelPiecesDebug();
        }

        private void RevisarWheelPiecesDebug()
        {
            Debug.Log($"🧪 Validando WheelPieces en {gameObject.name}...");

            if (wheelPieces == null || wheelPieces.Length == 0)
            {
                Debug.LogError("❌ wheelPieces vacío o nulo.");
                return;
            }

            for (int i = 0; i < wheelPieces.Length; i++)
            {
                var piece = wheelPieces[i];

                string label = piece.Label ?? "(sin label)";
                string iconStatus = piece.Icon != null ? "🖼️ icon ✅" : "❌ sin icon";
                string effectLabel = piece.Effect != null ? piece.Effect.label : "❌ null";
                string powerUpName = (piece.Effect != null && piece.Effect.powerUp != null) ? piece.Effect.powerUp.name : "❌ null";

                bool ok = piece.Effect != null && piece.Effect.powerUp != null && piece.Icon != null;

                string status = ok ? "✅ OK" : "⚠️ ERROR";

                
            }
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (wheelCircle == null || selector == null || wheelPieces == null || wheelPieces.Length == 0)
                return;

            Vector3 center = wheelCircle.position;

            Vector3 selectorDir = (selector.position - center).normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center, center + selectorDir * 2f);
            UnityEditor.Handles.Label(center + selectorDir * 2.2f, "📍 Selector");

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(selector.position, 0.1f);

            if (ultimoPremio != null)
            {
                int index = Array.IndexOf(wheelPieces, ultimoPremio);
                if (index >= 0 && index < wheelPiecesParent.childCount)
                {
                    Transform pieceTransform = wheelPiecesParent.GetChild(index).GetChild(0);
                    Vector3 winnerDir = (pieceTransform.position - center).normalized;
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(center, pieceTransform.position);
                    Gizmos.DrawSphere(pieceTransform.position, 0.1f);
                    UnityEditor.Handles.Label(pieceTransform.position + Vector3.up * 0.2f, $"🎁 {ultimoPremio.Label}");
                }
            }
#endif
        }
    }
}
