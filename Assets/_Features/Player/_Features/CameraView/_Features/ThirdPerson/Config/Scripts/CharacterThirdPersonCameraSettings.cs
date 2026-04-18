using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.ThirdPerson.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterThirdPersonCameraSettings", menuName = "BarafPani/Features/Player/Camera/Character Third Person Camera Settings")]
    public class CharacterThirdPersonCameraSettings : ScriptableObject
    {
        [Tooltip("Minimum pitch angle (looking down)"), Range(-90f, 0f)]
        public float PitchClampMin = -30f;
        [Tooltip("Maximum pitch angle (looking up)"), Range(0f, 90f)]
        public float PitchClampMax = 60f;
    }
}