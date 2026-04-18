using UnityEngine;

namespace _Features.Player._Features.Jump.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterJumpSettings", menuName = "BarafPani/Features/Player/Movement/Character Jump Settings")]
    public class CharacterJumpSettings : ScriptableObject
    {
        [Tooltip("Maximum height of a single jump in units"), Range(0.5f, 5f)]
        public float JumpHeight = 1.2f;
        [Tooltip("Downward acceleration applied each frame")]
        public float Gravity = -20f;
    }
}