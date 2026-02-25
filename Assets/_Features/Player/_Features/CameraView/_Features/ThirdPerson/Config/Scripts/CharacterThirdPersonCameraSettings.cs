using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.ThirdPerson.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterThirdPersonCameraSettings", menuName = "BarafPani/Features/Player/Camera/Character Third Person Camera Settings")]
    public class CharacterThirdPersonCameraSettings : ScriptableObject
    {
        public float PitchClampMin = -30f;
        public float PitchClampMax = 60f;
        public float RotationSmoothSpeed = 10f;
    }
}