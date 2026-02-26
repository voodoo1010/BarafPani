using _Features.Player._Features.Crouch.Config.Scripts;
using _Features.Player._Features.Walk.Scripts;
using _Features.Player.Scripts;
using UnityEngine;

namespace _Features.Player._Features.Crouch.Scripts
{
    public class CharacterCrouch : CharacterFeature
    {
        [SerializeField] private CharacterCrouchSettings crouchSettings;
        private bool IsCrouching { get; set; }

        private float _defaultHeight;
        private CharacterWalk _characterWalk;

        protected override void Awake()
        {
            base.Awake();
            _defaultHeight = Character.CharacterControllerUnityComponent.height;

        }
        private void OnEnable()
        {
            Character.OnCrouchInput += HandleCrouch;
        }
        private void OnDisable()
        {
            if (IsCrouching) SetCrouchState(false);
            Character.OnCrouchInput -= HandleCrouch;
        }
        private void HandleCrouch(bool pressed)
        {
            if (!pressed) return;
            SetCrouchState(!IsCrouching);
        }
        private void SetCrouchState(bool crouching)
        {
            IsCrouching = crouching;
            float targetHeight = crouching ? crouchSettings.CrouchHeight : _defaultHeight;
        }
    }
}