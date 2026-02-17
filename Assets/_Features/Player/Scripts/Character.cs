using System;
using UnityEngine;

namespace _Features.Player.Scripts
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterController characterControllerUnityComponent;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        public CharacterController CharacterControllerUnityComponent => characterControllerUnityComponent;

        public bool IsGrounded { get; private set; }

        private void Update()
        {
            CheckGround();
        }

        private void CheckGround()
        {
            IsGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
        }

        private void OnDrawGizmosSelected()
        {
            if (!groundCheckPoint) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        // Input events — published by CharacterInput, consumed by features
        public event Action<Vector2> OnMoveInput;
        public event Action<bool> OnSprintInput;
        public event Action<bool> OnCrouchInput;
        public event Action<bool> OnGrabInput;
        public event Action<bool> OnJumpInput;

        public void RaiseMoveInput(Vector2 input) => OnMoveInput?.Invoke(input);
        public void RaiseSprintInput(bool pressed) => OnSprintInput?.Invoke(pressed);
        public void RaiseCrouchInput(bool pressed) => OnCrouchInput?.Invoke(pressed);
        public void RaiseGrabInput(bool pressed) => OnGrabInput?.Invoke(pressed);
        public void RaiseJumpInput(bool pressed) => OnJumpInput?.Invoke(pressed);
    }
}