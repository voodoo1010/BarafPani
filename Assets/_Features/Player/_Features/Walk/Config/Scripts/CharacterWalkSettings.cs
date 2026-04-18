using UnityEngine;

namespace _Features.Player._Features.Walk.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterWalkSettings", menuName = "BarafPani/Features/Player/Movement/Character Walk Settings")]
    public class CharacterWalkSettings : ScriptableObject
    {
        [Tooltip("Base movement speed in units per second"), Range(1f, 20f)]
        public float Speed = 5f;
    }
}