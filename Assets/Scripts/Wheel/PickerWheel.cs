using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
using System;
using Unity.VisualScripting;

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
        [Header("Pools ponderados (elige 1 por porcentaje)")]
        [SerializeField] private List<WeightedPowerUpPool> powerUpPools = new List<WeightedPowerUpPool>();

        [Header("Legacy (solo si no hay pools ponderados)")]
        [SerializeField, Tooltip("LEGACY: se usa sólo si la lista de pools está vacía")]
        private PowerUpPool powerUpPool;

        [Header("Popup de recompensa")]
        [SerializeField] private RewardPopupUI rewardPopup;

        [Header("Referencias visuales")]
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private Transform linesParent;
        [SerializeField] private Transform selector;
        [SerializeField] private Transform PickerWheelTransform;
        [SerializeField] private Transform wheelCircle;
        [SerializeField] private GameObject wheelPiecePrefab;
        [SerializeField] private Transform wheelPiecesParent;

        [Header("Sonidos")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip tickAudioClip;
        [Range(0f, 1f)][SerializeField] private float volume = .5f;
        [Range(-3f, 3f)][SerializeField] private float pitch = 1f;

        [Header("Configuración")]
        [Range(1, 20)] public int spinDuration = 8;
        [Range(.2f, 2f)][SerializeField] private float wheelSize = 1f;

        [Header("Premios")]
        public WheelPiece[] wheelPieces;

        [Header("Usos disponibles")]
        [SerializeField] private int usosMaximos = 3;
        private int usosRestantes;
        public int UsosRestantes => usosRestantes;

        private bool _isSpinning = false;
        public bool IsSpinning => _isSpinning;

        private float pieceAngle;
        private float halfPieceAngle;
        private float halfPieceAngleWithPaddings;
        private double accumulatedWeight;
        private System.Random rand = new System.Random();
        private List<int> nonZeroChancesIndices = new List<int>();
        private WheelPiece ultimoPremio;
        private float ruletaUltimoAngulo = 0f;

        public WheelPiece ObtenerUltimoPremio() => ultimoPremio;


        public Action<WheelPiece> OnSpinEnd;
        private UnityAction onSpinStartEvent;
        private UnityAction<WheelPiece> onSpinEndEvent;

        private Vector2 pieceMinSize = new Vector2(81f, 146f);
        private Vector2 pieceMaxSize = new Vector2(144f, 213f);
        private int piecesMin = 2;
        private int piecesMax = 12;

        private void Awake()
        {
            usosRestantes = usosMaximos;
        }

        private void Start()
        {
            // ✅ Buscar automáticamente el popup si no está asignado
            if (rewardPopup == null)
            {
                rewardPopup = FindObjectOfType<RewardPopupUI>(true);
                if (rewardPopup != null)
                    Debug.Log($"🔍 RewardPopupUI encontrado automáticamente: {rewardPopup.name}");
                else
                    Debug.LogWarning("⚠️ No se encontró ningún RewardPopupUI en la escena.");
            }

            // Carga inicial de premios
            CargarPremiosDesdePoolsPonderados();

            pieceAngle = 360f / wheelPieces.Length;
            halfPieceAngle = pieceAngle / 2f;
            halfPieceAngleWithPaddings = halfPieceAngle - (halfPieceAngle / 4f);

            Generate();
            CalculateWeightsAndIndices();

            if (nonZeroChancesIndices.Count == 0)
                Debug.LogError("❌ No se pueden tener todas las piezas con chance 0.");

            SetupAudio();
        }

        private void SetupAudio()
        {
            audioSource.clip = tickAudioClip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
        }

        private void OnRewardSelected(WheelPiece selectedPiece)
        {
            Sprite sprite = selectedPiece.Icon;
            string name = selectedPiece.Label;
            string desc = selectedPiece.Effect != null ? selectedPiece.Effect.description : "Sin descripción";

            if (rewardPopup != null)
                rewardPopup.ShowReward(sprite, name, desc);
            else
                Debug.LogWarning("⚠️ No se encontró RewardPopupUI para mostrar la descripción.");
        }

        // ----------------------------------------------------
        // 🌀 Lógica principal de la ruleta
        // ----------------------------------------------------
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
                .SetEase(Ease.OutQuad)
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
                    Debug.Log($"🎯 Selector está sobre la pieza {landedIndex}: {ultimoPremio.Label}");

                    // Llamar evento y popup
                    OnRewardSelected(ultimoPremio);
                    OnSpinEnd?.Invoke(ultimoPremio);
                    onSpinEndEvent?.Invoke(ultimoPremio);
                });
        }

        // ----------------------------------------------------
        // ⚙️ Métodos utilitarios
        // ----------------------------------------------------
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

        private void Generate()
        {
            // Limpieza inicial para evitar acumulación de piezas duplicadas
            foreach (Transform child in wheelPiecesParent)
                Destroy(child.gameObject);
            foreach (Transform child in linesParent)
                Destroy(child.gameObject);

            // Calcular tamaño base según cantidad de piezas
            float t = Mathf.InverseLerp(piecesMin, piecesMax, Mathf.Clamp(wheelPieces.Length, piecesMin, piecesMax));
            float pieceWidth = Mathf.Lerp(pieceMaxSize.x, pieceMinSize.x, t);
            float pieceHeight = Mathf.Lerp(pieceMaxSize.y, pieceMinSize.y, t);

            for (int i = 0; i < wheelPieces.Length; i++)
            {
                WheelPiece piece = wheelPieces[i];

                // Instanciar pieza
                GameObject pieceObj = Instantiate(wheelPiecePrefab, wheelPiecesParent);
                Transform pieceTrns = pieceObj.transform.GetChild(0);

                // Asignar sprite, label y cantidad
                pieceTrns.GetChild(0).GetComponent<Image>().sprite = piece.Icon;
                pieceTrns.GetChild(1).GetComponent<Text>().text = piece.Label;
                pieceTrns.GetChild(2).GetComponent<Text>().text = piece.Amount.ToString();

                // Ajustar tamaño del RectTransform sin deformar
                RectTransform rt = pieceTrns.GetComponent<RectTransform>();
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pieceWidth);
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pieceHeight);
                rt.localScale = Vector3.one; // 🔧 evita escalado acumulativo

                // Calcular rotación de la pieza
                pieceTrns.RotateAround(wheelPiecesParent.position, Vector3.back, pieceAngle * i);

                // Crear la línea divisoria
                Transform lineTrns = Instantiate(linePrefab, linesParent.position, Quaternion.identity, linesParent).transform;
                lineTrns.RotateAround(wheelPiecesParent.position, Vector3.back, (pieceAngle * i) + halfPieceAngle);
            }
        }
        private void DrawPiece(int index)
        {
            WheelPiece piece = wheelPieces[index];
            Transform pieceTrns = InstantiatePiece().transform.GetChild(0);

            // Solo mostrar el ícono
            Image iconImage = pieceTrns.GetChild(0).GetComponent<Image>();
            iconImage.sprite = piece.Icon;

            // Ocultar el texto del label y cantidad si existen
            Transform labelText = pieceTrns.childCount > 1 ? pieceTrns.GetChild(1) : null;
            Transform amountText = pieceTrns.childCount > 2 ? pieceTrns.GetChild(2) : null;

            if (labelText != null)
                labelText.gameObject.SetActive(false);
            if (amountText != null)
                amountText.gameObject.SetActive(false);

            // Posicionar el icono
            pieceTrns.RotateAround(wheelPiecesParent.position, Vector3.back, pieceAngle * index);

            // Crear la línea divisoria
            Transform lineTrns = Instantiate(linePrefab, linesParent.position, Quaternion.identity, linesParent).transform;
            lineTrns.RotateAround(wheelPiecesParent.position, Vector3.back, (pieceAngle * index) + halfPieceAngle);
        }

        private GameObject InstantiatePiece() => Instantiate(wheelPiecePrefab, wheelPiecesParent);

        // ----------------------------------------------------
        // 🧩 Aplicar premio al jugador
        // ----------------------------------------------------
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
        public void CargarPremiosDesdePoolsPonderados()
        {
            PowerUpPool elegido = null;

            // Buscar el pool ponderado elegido
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
                    // Fallback si algo sale mal
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
                // Legacy: usa el pool único
                elegido = powerUpPool;
                if (elegido != null)
                    Debug.Log($"(LEGACY) Usando PowerUpPool: {elegido.name}");
            }

            // Validaciones
            if (elegido == null || elegido.entries == null || elegido.entries.Length == 0)
            {
                Debug.LogError("❌ No hay PowerUpPool válido o está vacío.");
                return;
            }

            // Construir las piezas de la ruleta
            wheelPieces = new WheelPiece[elegido.entries.Length];

            for (int i = 0; i < elegido.entries.Length; i++)
            {
                PowerUpEntry entry = elegido.entries[i];

                if (entry == null || entry.effect == null)
                {
                    Debug.LogError($"❌ Entrada nula o sin efecto en índice {i}");
                    continue;
                }

                wheelPieces[i] = new WheelPiece
                {
                    Icon = entry.effect.icon,
                    Label = "", // ❌ no mostrar texto en la ruleta
                    Amount = 1,
                    Chance = entry.chance,
                    Effect = entry.effect
                };

            }

            // Redibujar la ruleta
            foreach (Transform child in wheelPiecesParent)
                Destroy(child.gameObject);
            foreach (Transform child in linesParent)
                Destroy(child.gameObject);

            Generate();
            CalculateWeightsAndIndices();
        }
        public void MostrarPopupUltimoPremio()
        {
            if (ultimoPremio == null)
            {
                Debug.LogWarning("⚠️ No hay premio disponible para mostrar en el popup.");
                return;
            }

            // Si el popup no fue asignado manualmente, intenta encontrarlo automáticamente
            if (rewardPopup == null)
            {
                rewardPopup = FindObjectOfType<RewardPopupUI>(true);
                if (rewardPopup == null)
                {
                    Debug.LogWarning("⚠️ No se encontró RewardPopupUI en la escena.");
                    return;
                }
            }

            Sprite sprite = ultimoPremio.Icon;
            string name = ultimoPremio.Label;
            string desc = ultimoPremio.Effect != null ? ultimoPremio.Effect.description : "Sin descripción";

            rewardPopup.ShowReward(sprite, name, desc);
        }
        public void AddSpinStartListener(UnityAction callback) => onSpinStartEvent += callback;
        public void AddSpinEndListener(UnityAction<WheelPiece> callback) => onSpinEndEvent += callback;
    }

}
