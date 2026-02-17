using _Features.Player.Scripts;
using UnityEngine;

namespace _Features.Player._Features.Jump.Scripts
{
    public class CharacterJump : CharacterFeature
    {
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        private float _verticalVelocity;

        private void OnEnable()
        {
            Character.OnJumpInput += HandleJump;
        }

        private void OnDisable()
        {
            _verticalVelocity = 0f;
            Character.OnJumpInput -= HandleJump;
        }

        private void Update()
        {
            if (Character.IsGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _verticalVelocity += gravity * Time.deltaTime;
            Character.CharacterControllerUnityComponent.Move(new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));
        }

        private void HandleJump(bool pressed)
        {
            if (!pressed) return;
            if (!Character.IsGrounded) return;

            _verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }
    }
}