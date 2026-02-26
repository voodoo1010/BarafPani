using UnityEngine;

namespace _Features.Player._Features.Crouch.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterCrouchSettings", menuName = "BarafPani/Features/Player/Movement/Character Crouch Settings")]
    public class CharacterCrouchSettings : ScriptableObject
    {
        public float CrouchHeight = 1f;
        public float TransitionDuration = 0.3f;
        public float CrouchSpeedMultiplier = 0.5f;
    }
}