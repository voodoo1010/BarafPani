using System;
using _Features.Player.Config.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Player.Scripts
{
    public class Character : MonoBehaviour
    {
        [SerializeField] private CharacterController characterControllerUnityComponent;
        [SerializeField] private CharacterSettings characterSettings;
        [HorizontalLine("Ground Check Specific", 1, FixedColor.Black)]
        [MessageBox("Kept this here so the position is taken from a child within the GameObject, keeps positions local.", MessageBoxType.Info)]
        [SerializeField] private Transform groundCheckTransform;

        public CharacterController CharacterControllerUnityComponent => characterControllerUnityComponent;

        public bool IsGrounded { get; private set; }

        private void Update()
        {
            CheckGround();
        }

        private void CheckGround()
        {
            IsGrounded = Physics.CheckSphere(groundCheckTransform.position, characterSettings.GroundCheckRadius, characterSettings.GroundLayer);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckTransform.position, characterSettings.GroundCheckRadius);
        }
        public Transform CameraTransform { get; set; }

        public event Action<Vector2> OnMoveInput;
        public event Action<Vector2> OnLookInput;
        public event Action<bool> OnSprintInput;
        public event Action<bool> OnCrouchInput;
        public event Action<bool> OnGrabInput;
        public event Action<bool> OnJumpInput;

        public void RaiseMoveInput(Vector2 input) => OnMoveInput?.Invoke(input);
        public void RaiseLookInput(Vector2 delta) => OnLookInput?.Invoke(delta);
        public void RaiseSprintInput(bool pressed) => OnSprintInput?.Invoke(pressed);
        public void RaiseCrouchInput(bool pressed) => OnCrouchInput?.Invoke(pressed);
        public void RaiseGrabInput(bool pressed) => OnGrabInput?.Invoke(pressed);
        public void RaiseJumpInput(bool pressed) => OnJumpInput?.Invoke(pressed);
    }
}