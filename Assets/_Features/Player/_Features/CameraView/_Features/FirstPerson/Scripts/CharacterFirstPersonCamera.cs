using _Features.Player._Features.CameraView._Features.FirstPerson.Config.Scripts;
using _Features.Player._Features.CameraView.Scripts;
using CustomInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.FirstPerson.Scripts
{
    public class CharacterFirstPersonCamera : CharacterCameraView
    {
        [SerializeField, ForceFill, Tooltip("ScriptableObject with pitch clamp and eye height settings")]
        private CharacterFirstPersonCameraSettings firstPersonSettings;

        private CinemachinePanTilt _panTilt;
        private Transform _eyePivot;
        protected override void Awake()
        {
            base.Awake();

            _eyePivot = new GameObject("[EyePivot]").transform;
            _eyePivot.SetParent(Character.transform);
            _eyePivot.localPosition = firstPersonSettings.EyeHeight;
            _eyePivot.localRotation = Quaternion.identity;

            CinemachineCamera.Follow = _eyePivot;

            var follow = CinemachineCamera.GetComponent<CinemachineFollow>();
            if (follow != null)
                DestroyImmediate(follow);

            CinemachineCamera.gameObject.AddComponent<CinemachineHardLockToTarget>();

            _panTilt = CinemachineCamera.GetComponent<CinemachinePanTilt>();
            _panTilt.ReferenceFrame = CinemachinePanTilt.ReferenceFrames.World;
        }

        protected override void ApplyLook(float yaw, float pitch)
        {
            _panTilt.PanAxis.Value += yaw;
            _panTilt.TiltAxis.Value = Mathf.Clamp(
                _panTilt.TiltAxis.Value + pitch,
                firstPersonSettings.PitchClampMin,
                firstPersonSettings.PitchClampMax
            );

            Character.transform.rotation = Quaternion.Euler(0f, _panTilt.PanAxis.Value, 0f);

            ApplyLookDownOffset();
        }

        private void ApplyLookDownOffset()
        {
            // Remove the minus sign — TiltAxis is positive when looking down in your setup
            float lookDownAngle = Mathf.Clamp(_panTilt.TiltAxis.Value, 0f, 90f);
            float strength = firstPersonSettings.LookDownOffsetCurve.Evaluate(lookDownAngle);

            _eyePivot.localPosition = firstPersonSettings.EyeHeight
                + firstPersonSettings.LookDownOffset * strength;
        }
    }
}