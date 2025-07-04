﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
using System;

namespace EasyUI.PickerWheelUI
{
    public class PickerWheel : MonoBehaviour
    {
        [Header("Referencias :")]
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private Transform linesParent;

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

        private void Generate()
        {
            wheelPiecePrefab = InstantiatePiece();
            RectTransform rt = wheelPiecePrefab.transform.GetChild(0).GetComponent<RectTransform>();
            float pieceWidth = Mathf.Lerp(pieceMinSize.x, pieceMaxSize.x, 1f - Mathf.InverseLerp(piecesMin, piecesMax, wheelPieces.Length));
            float pieceHeight = Mathf.Lerp(pieceMinSize.y, pieceMaxSize.y, 1f - Mathf.InverseLerp(piecesMin, piecesMax, wheelPieces.Length));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pieceWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pieceHeight);

            for (int i = 0; i < wheelPieces.Length; i++)
                DrawPiece(i);

            Destroy(wheelPiecePrefab);
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

        // ✅ MODIFICADO: ya no toca posición mundial
        private GameObject InstantiatePiece()
        {
            return Instantiate(wheelPiecePrefab, wheelPiecesParent); // solo se asigna el padre
        }

        public void Spin()
        {
            if (_isSpinning)
                return;

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
            Vector3 targetRotation = Vector3.back * totalRotation;

            float prevAngle = wheelCircle.eulerAngles.z;
            float currentAngle = prevAngle;
            bool isIndicatorOnLine = false;

            wheelCircle
                .DORotate(targetRotation, spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutQuart)
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
                    ultimoPremio = piece;
                    usosRestantes--;

                    if (usosRestantes <= 0)
                    {
                        Debug.Log($"{gameObject.name} se quedó sin usos.");
                    }

                    OnSpinEnd?.Invoke(piece);
                    onSpinEndEvent?.Invoke(piece);
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

            if (wheelPieces.Length > piecesMax || wheelPieces.Length < piecesMin)
                Debug.LogError("[ PickerWheel ] pieces length must be between " + piecesMin + " and " + piecesMax);
        }
    }
}
