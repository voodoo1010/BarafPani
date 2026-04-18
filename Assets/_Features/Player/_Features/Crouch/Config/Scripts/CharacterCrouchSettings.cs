using UnityEngine;

namespace _Features.Player._Features.Crouch.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterCrouchSettings", menuName = "BarafPani/Features/Player/Movement/Character Crouch Settings")]
    public class CharacterCrouchSettings : ScriptableObject
    {
        [Tooltip("CharacterController height while crouched"), Range(0.5f, 2f)]
        public float CrouchHeight = 1f;
        [Tooltip("Seconds to interpolate between standing and crouching"), Range(0.05f, 1f)]
        public float TransitionDuration = 0.3f;
        [Tooltip("Speed multiplier applied while crouching (0-1)"), Range(0f, 1f)]
        public float CrouchSpeedMultiplier = 0.5f;
    }
}