using UnityEngine;

namespace _Features.Player.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "BarafPani/Features/Player/Movement/Character Settings")]
    public class CharacterSettings : ScriptableObject
    {
        public float GroundCheckRadius = 0.2f;
        public LayerMask GroundLayer;

    }
}