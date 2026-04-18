using UnityEngine;

namespace _Features.Player._Features.Sprint.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterSprintSettings", menuName = "BarafPani/Features/Player/Movement/Character Sprint Settings")]
    public class CharacterSprintSettings : ScriptableObject
    {
        [Tooltip("Multiplier applied to base walk speed while sprinting"), Range(1f, 3f)]
        public float SprintMultiplier = 1.5f;
    }
}