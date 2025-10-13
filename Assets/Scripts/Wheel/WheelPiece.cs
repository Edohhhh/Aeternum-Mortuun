using UnityEngine;

namespace EasyUI.PickerWheelUI
{
    [System.Serializable]
    public class WheelPiece
    {
        public Sprite Icon;
        public string Label;
        public PowerUpEffect Effect; // ✅ referencia al ScriptableObject

        [Tooltip("Reward amount")]
        public int Amount = 1;

        [Tooltip("Probability in %")]
        [Range(0f, 100f)]
        public float Chance = 100f;

        [HideInInspector] public int Index;
        [HideInInspector] public double _weight = 0f;
    }
}
