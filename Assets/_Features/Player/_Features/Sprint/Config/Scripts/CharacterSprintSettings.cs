using UnityEngine;

namespace _Features.Player._Features.Sprint.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterSprintSettings", menuName = "BarafPani/Features/Player/Movement/Character Sprint Settings")]
    public class CharacterSprintSettings : ScriptableObject
    {
        public float SprintMultiplier = 1.5f;

    }
}