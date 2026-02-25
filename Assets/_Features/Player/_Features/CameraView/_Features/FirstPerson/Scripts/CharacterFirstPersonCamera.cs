using _Features.Player._Features.CameraView._Features.FirstPerson.Config.Scripts;
using _Features.Player._Features.CameraView.Scripts;
using Unity.Cinemachine;
using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.FirstPerson.Scripts
{
    public class CharacterFirstPersonCamera : CharacterCameraView
    {
        [SerializeField] private CharacterFirstPersonCameraSettings firstPersonSettings;

        private CinemachinePanTilt _panTilt;

        protected override void Awake()
        {
            base.Awake();

            CinemachineCamera.Follow = Character.transform;

            var follow = CinemachineCamera.GetComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, firstPersonSettings.EyeHeight, 0f);

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
        }
    }
}