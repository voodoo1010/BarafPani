using UnityEngine;

namespace _Features.Player._Features.CameraView.Config.Scripts
{
    [CreateAssetMenu(fileName = "CharacterCameraViewSettings", menuName = "BarafPani/Features/Player/Camera/Character Camera View Settings")]
    public class CharacterCameraViewSettings : ScriptableObject
    {
        [Tooltip("Horizontal look sensitivity (yaw)"), Range(0.01f, 1f)]
        public float HorizontalSensitivity = 0.1f;
        [Tooltip("Vertical look sensitivity (pitch)"), Range(0.01f, 1f)]
        public float VerticalSensitivity = 0.1f;
    }
}