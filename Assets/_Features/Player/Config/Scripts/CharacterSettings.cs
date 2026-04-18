using UnityEngine;

namespace _Features.Player.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "BarafPani/Features/Player/Movement/Character Settings")]
    public class CharacterSettings : ScriptableObject
    {
        [Tooltip("Radius of the ground check sphere cast"), Range(0.01f, 1f)]
        public float GroundCheckRadius = 0.2f;
        [Tooltip("Layers considered as walkable ground")]
        public LayerMask GroundLayer;
    }
}