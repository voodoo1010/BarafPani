using _Features.Player._Features.Walk.Config.Scripts;
using _Features.Player.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Player._Features.Walk.Scripts
{
    public class CharacterWalk : CharacterFeature, IMovementDataProvider
    {
        [SerializeField, ForceFill, Tooltip("ScriptableObject with walk speed configuration")]
        private CharacterWalkSettings characterWalkSettings;

        [SerializeField, Tooltip("How fast the character rotates to face movement direction (degrees per second)")]
        private float rotationSpeed = 720f;

        public float SpeedMultiplier { get; set; } = 1f;
        public float CrouchSpeedMultiplier { get; set; } = 1f;

        private Vector2 _moveInput;
        public Vector2 MoveInput => _moveInput;

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

        private void LateUpdate()
        {
            if (_moveInput == Vector2.zero) return;

            Vector3 direction = GetMoveDirection();

            Character.CharacterControllerUnityComponent.Move(
                direction * (characterWalkSettings.Speed * SpeedMultiplier * CrouchSpeedMultiplier * Time.deltaTime)
            );

            RotateTowardsMovement(direction);
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

        private void RotateTowardsMovement(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            Character.transform.rotation = Quaternion.RotateTowards(
                Character.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        private void HandleMove(Vector2 input)
        {
            _moveInput = input;
        }
    }
}