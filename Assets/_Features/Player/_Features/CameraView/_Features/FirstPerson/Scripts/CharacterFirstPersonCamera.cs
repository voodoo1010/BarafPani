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

        protected override void Awake()
        {
            base.Awake();

            var eyePivot = new GameObject("[EyePivot]").transform;
            eyePivot.SetParent(Character.transform);
            eyePivot.localPosition = firstPersonSettings.EyeHeight; // full Vector3, set by you
            eyePivot.localRotation = Quaternion.identity;

            CinemachineCamera.Follow = eyePivot;

            var follow = CinemachineCamera.GetComponent<CinemachineFollow>();
            if (follow != null)
                DestroyImmediate(follow); // immediate so AddComponent below doesn't conflict

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

            // Body rotates yaw-only; camera pitch is handled entirely by PanTilt
            Character.transform.rotation = Quaternion.Euler(0f, _panTilt.PanAxis.Value, 0f);
        }
    }
}