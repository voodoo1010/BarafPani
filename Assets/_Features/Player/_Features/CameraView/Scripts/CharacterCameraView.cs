using _Features.Player._Features.CameraView.Config.Scripts;
using _Features.Player.Scripts;
using Unity.Cinemachine;
using UnityEngine;

namespace _Features.Player._Features.CameraView.Scripts
{
    public abstract class CharacterCameraView : CharacterFeature
    {
        [SerializeField] private CharacterCameraViewSettings cameraViewSettings;
        [SerializeField] private CinemachineCamera cinemachineCamera;

        protected CharacterCameraViewSettings CameraViewSettings => cameraViewSettings;
        protected CinemachineCamera CinemachineCamera => cinemachineCamera;
        protected virtual Transform MovementReferenceTransform => cinemachineCamera.transform;

        private void OnEnable()
        {
            Character.OnLookInput += HandleLookInput;
            Character.CameraTransform = MovementReferenceTransform;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Character.OnLookInput -= HandleLookInput;
            if (Character.CameraTransform == MovementReferenceTransform)
                Character.CameraTransform = null;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleLookInput(Vector2 delta)
        {
            float scaledX = delta.x * cameraViewSettings.HorizontalSensitivity;
            float scaledY = delta.y * cameraViewSettings.VerticalSensitivity;
            ApplyLook(scaledX, scaledY);
        }

        protected abstract void ApplyLook(float yaw, float pitch);
    }
}