using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.FirstPerson.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterFirstPersonCameraSettings", menuName = "BarafPani/Features/Player/Camera/Character First Person Camera Settings")]
    public class CharacterFirstPersonCameraSettings : ScriptableObject
    {
        [Tooltip("Minimum pitch angle (looking down)"), Range(-90f, 0f)]
        public float PitchClampMin = -80f;
        [Tooltip("Maximum pitch angle (looking up)"), Range(0f, 90f)]
        public float PitchClampMax = 80f;
        [Tooltip("Camera height from character origin in units")]
        public Vector3 EyeHeight;
    }
}
