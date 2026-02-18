using UnityEngine;

namespace _Features.Player._Features.Jump.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterJumpSettings", menuName = "BarafPani/Features/Player/Movement/Character Sprint Settings")]
    public class CharacterJumpSettings : ScriptableObject
    {
        public float JumpHeight = 1.2f;
        public float Gravity = -20f;
    }
}