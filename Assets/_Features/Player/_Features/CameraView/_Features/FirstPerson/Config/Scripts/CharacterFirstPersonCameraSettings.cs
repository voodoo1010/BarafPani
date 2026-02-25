using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.FirstPerson.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterFirstPersonCameraSettings", menuName = "BarafPani/Features/Player/Camera/Character First Person Camera Settings")]
    public class CharacterFirstPersonCameraSettings : ScriptableObject
    {
        public float PitchClampMin = -80f;
        public float PitchClampMax = 80f;
    }
}
