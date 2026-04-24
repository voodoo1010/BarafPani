using System;
using _Features.Player._Features.Jump.Config.Scripts;
using _Features.Player.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Player._Features.Jump.Scripts
{
    public class CharacterJump : CharacterFeature
    {
        [SerializeField, ForceFill, Tooltip("ScriptableObject with jump height and gravity settings")]
        private CharacterJumpSettings characterJumpSettings;

        private float _verticalVelocity;

        public event Action JumpStarted;

        public bool IsAscending => !Character.IsGrounded && _verticalVelocity > 0f;
        public bool IsFalling => !Character.IsGrounded && _verticalVelocity <= 0f;

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

            _verticalVelocity += characterJumpSettings.Gravity * Time.deltaTime;
            Character.CharacterControllerUnityComponent.Move(new Vector3(0f, _verticalVelocity * Time.deltaTime, 0f));
        }

        private void HandleJump(bool pressed)
        {
            if (!pressed) return;
            if (!Character.IsGrounded) return;

            _verticalVelocity = Mathf.Sqrt(-2f * characterJumpSettings.Gravity * characterJumpSettings.JumpHeight);
            JumpStarted?.Invoke();
        }
    }
}
