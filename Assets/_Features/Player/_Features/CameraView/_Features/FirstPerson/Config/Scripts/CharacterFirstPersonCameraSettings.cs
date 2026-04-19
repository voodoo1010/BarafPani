using CustomInspector;
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
        [Tooltip("Camera offset from character origin (X = side, Y = height, Z = forward)")]
        public Vector3 EyeHeight = new Vector3(0f, 1.7f, 0f);

        [HorizontalLine("Look Down Body Offset", 1, FixedColor.Gray)]
        [Tooltip("How much to offset the camera (Z = back, Y = up) at full look-down")]
        public Vector3 LookDownOffset = new Vector3(0f, 0.1f, -0.3f);

        [Tooltip("Curve controlling offset strength vs look-down angle (X: 0-90 degrees, Y: 0-1 strength)")]
        public AnimationCurve LookDownOffsetCurve = AnimationCurve.Linear(0f, 0f, 90f, 1f);
    }
}
