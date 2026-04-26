using CustomInspector;
using UnityEngine;

namespace _Features.Player._Features.Freeze.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterFreezeSettings", menuName = "BarafPani/Features/Player/Status/Character Freeze Settings")]
    public class CharacterFreezeSettings : ScriptableObject
    {
        [HorizontalLine("Factor", 1, FixedColor.Cyan)]
        [Tooltip("Threshold at which the character becomes fully frozen (hard-locked)"), Range(1f, 500f)]
        public float MaxFreezeFactor = 100f;

        [Tooltip("How much the freeze factor decays per second when no new hits land"), Range(0f, 100f)]
        public float FreezeDecayPerSecond = 10f;

        [HorizontalLine("Slowdown", 1, FixedColor.Cyan)]
        [Tooltip("Speed multiplier when the freeze factor is at the maximum (just before hard-lock triggers)"), Range(0f, 1f)]
        public float MinSlowMultiplier = 0.2f;

        [Tooltip("Maps normalized freeze factor (0..1) to the slowdown lerp t. Linear by default.")]
        public AnimationCurve SlowCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [HorizontalLine("Hard Lock", 1, FixedColor.Cyan)]
        [Tooltip("Seconds the character is fully frozen once the threshold is reached"), Range(0.1f, 15f)]
        public float FullFreezeDuration = 3f;
    }
}
