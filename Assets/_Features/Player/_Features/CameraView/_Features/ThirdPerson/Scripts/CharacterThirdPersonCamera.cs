using _Features.Player._Features.CameraView._Features.ThirdPerson.Config.Scripts;
using _Features.Player._Features.CameraView.Scripts;
using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.ThirdPerson.Scripts
{
    public class CharacterThirdPersonCamera : CharacterCameraView
    {
        [SerializeField] private CharacterThirdPersonCameraSettings thirdPersonSettings;

        private float _yaw;
        private float _pitch;
        private Vector2 _moveInput;

        protected override void Awake()
        {
            base.Awake();
            Character.OnMoveInput += HandleMoveInput;
        }

        private void OnDestroy()
        {
            Character.OnMoveInput -= HandleMoveInput;
        }

        protected override void ApplyLook(float yaw, float pitch)
        {
            _yaw += yaw;
            _pitch -= pitch;
            _pitch = Mathf.Clamp(_pitch, thirdPersonSettings.PitchClampMin, thirdPersonSettings.PitchClampMax);

            CinemachineCamera.transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        private void LateUpdate()
        {
            RotateCharacterTowardMovement();
        }

        private void RotateCharacterTowardMovement()
        {
            if (_moveInput == Vector2.zero) return;

            Transform cam = CinemachineCamera.transform;
            Vector3 forward = cam.forward;
            Vector3 right = cam.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 targetDirection = forward * _moveInput.y + right * _moveInput.x;
            if (targetDirection.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Character.transform.rotation = Quaternion.Slerp(
                Character.transform.rotation,
                targetRotation,
                thirdPersonSettings.RotationSmoothSpeed * Time.deltaTime
            );
        }

        private void HandleMoveInput(Vector2 input)
        {
            _moveInput = input;
        }
    }
}