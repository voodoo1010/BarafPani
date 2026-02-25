using _Features.Player._Features.CameraView._Features.FirstPerson.Config.Scripts;
using _Features.Player._Features.CameraView.Scripts;
using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.FirstPerson.Scripts
{
    public class CharacterFirstPersonCamera : CharacterCameraView
    {
        [SerializeField] private CharacterFirstPersonCameraSettings firstPersonSettings;

        private float _pitch;

        protected override void ApplyLook(float yaw, float pitch)
        {
            Character.transform.Rotate(Vector3.up, yaw);

            _pitch -= pitch;
            _pitch = Mathf.Clamp(_pitch, firstPersonSettings.PitchClampMin, firstPersonSettings.PitchClampMax);

            CinemachineCamera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}
