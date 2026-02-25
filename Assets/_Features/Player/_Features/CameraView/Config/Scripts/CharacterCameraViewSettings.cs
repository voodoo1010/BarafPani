using UnityEngine;

namespace _Features.Player._Features.CameraView.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterCameraViewSettings", menuName = "BarafPani/Features/Player/Camera/Character Camera View Settings")]
    public class CharacterCameraViewSettings : ScriptableObject
    {
        public float HorizontalSensitivity = 0.1f;
        public float VerticalSensitivity = 0.1f;
    }
}