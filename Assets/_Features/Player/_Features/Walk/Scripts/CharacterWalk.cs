using _Features.Player._Features.Walk.Config.Scripts;
using _Features.Player.Scripts;
using UnityEngine;

namespace _Features.Player._Features.Walk.Scripts
{
    public class CharacterWalk : CharacterFeature
    {
        [SerializeField] private CharacterWalkSettings characterWalkSettings;
        //TODO: put this data shit in scriptableojects
        public float SpeedMultiplier { get; set; } = 1f;
        public float CrouchSpeedMultiplier { get; set; } = 1f;

        private Vector2 _moveInput;

        private void OnEnable()
        {
            Character.OnMoveInput += HandleMove;
        }

        private void OnDisable()
        {
            _moveInput = Vector2.zero;
            SpeedMultiplier = 1f;
            CrouchSpeedMultiplier = 1f;
            Character.OnMoveInput -= HandleMove;
        }

        private void Update()
        {
            if (_moveInput == Vector2.zero) return;

            Vector3 direction = GetMoveDirection();
            Character.CharacterControllerUnityComponent.Move(direction * (characterWalkSettings.Speed * SpeedMultiplier * CrouchSpeedMultiplier * Time.deltaTime));
        }

        private Vector3 GetMoveDirection()
        {
            Transform cam = Character.CameraTransform;
            if (!cam) return new Vector3(_moveInput.x, 0f, _moveInput.y);

            Vector3 forward = cam.forward;
            Vector3 right = cam.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return forward * _moveInput.y + right * _moveInput.x;
        }

        private void HandleMove(Vector2 input)
        {
            _moveInput = input;
        }
    }
}