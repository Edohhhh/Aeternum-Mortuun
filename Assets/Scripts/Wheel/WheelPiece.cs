using UnityEngine;

namespace EasyUI.PickerWheelUI
{
    [System.Serializable]
    public class WheelPiece
    {
        public Sprite Icon;
        public string Label;

        [Tooltip("Reward amount")]
        public int Amount = 1;

        [Tooltip("Probability in %")]
        [Range(0f, 100f)]
        public float Chance = 100f;

        [HideInInspector] public int Index;
        [HideInInspector] public double _weight = 0f;

        // 🔁 Nuevo: Referencia directa al efecto funcional
        [HideInInspector] public PowerUpEffect Effect;
    }
}