using _Features.Player._Features.Crouch.Scripts;
using _Features.Player._Features.Jump.Scripts;
using _Features.Player._Features.Walk.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Player.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class RunnerAnimationController : MonoBehaviour
    {
        private static readonly int BlendHash = Animator.StringToHash("Blend");
        private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");

        [HorizontalLine("Smoothing", 1, FixedColor.Cyan)]
        [SerializeField, Range(0.1f, 50f), Tooltip("Exponential blend speed for locomotion transitions. Higher = snappier.")]
        private float blendSmoothing = 10f;

        [HorizontalLine("Debug", 1, FixedColor.Black)]
        [MessageBox("Runtime-only values. Visible for tuning.", MessageBoxType.Info)]
        [SerializeField, ReadOnly, Tooltip("Smoothed locomotion blend sent to the Animator")]
        private float currentBlend;

        private Animator _animator;
        private Character _character;
        private IMovementDataProvider _movementDataProvider;
        private ICrouchDataProvider _crouchDataProvider;
        private CharacterJump _characterJump;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _character = GetComponentInParent<Character>();
            _movementDataProvider = GetComponentInParent<CharacterWalk>();
            _crouchDataProvider = GetComponentInParent<CharacterCrouch>();
            _characterJump = GetComponentInParent<CharacterJump>();
        }

        private void OnEnable()
        {
            if (_characterJump != null)
                _characterJump.JumpStarted += HandleJumpStarted;
        }

        private void OnDisable()
        {
            if (_characterJump != null)
                _characterJump.JumpStarted -= HandleJumpStarted;
        }

        private void Update()
        {
            UpdateLocomotion();
            UpdateCrouch();
            UpdateGrounded();
        }

        private void UpdateLocomotion()
        {
            float targetBlend = _movementDataProvider != null ? Mathf.Clamp01(_movementDataProvider.MoveInput.magnitude) : 0f;

            float smoothFactor = 1f - Mathf.Exp(-blendSmoothing * Time.deltaTime);
            currentBlend = Mathf.Lerp(currentBlend, targetBlend, smoothFactor);

            if (targetBlend < 0.01f && currentBlend < 0.001f)
                currentBlend = 0f;

            _animator.SetFloat(BlendHash, currentBlend);
        }

        private void UpdateCrouch()
        {
            bool crouching = _crouchDataProvider != null && _crouchDataProvider.IsCrouching;
            _animator.SetBool(IsCrouchingHash, crouching);
        }

        private void UpdateGrounded()
        {
            bool grounded = _character != null && _character.IsGrounded;
            _animator.SetBool(IsGroundedHash, grounded);
        }

        private void HandleJumpStarted()
        {
            _animator.SetTrigger(JumpTriggerHash);
        }
    }
}
