using _Features.Player.Scripts;
using UnityEngine;

namespace _Features.Player._Features.Walk.Scripts
{
    public class CharacterWalk : CharacterFeature
    {
        [SerializeField] private float speed = 5f;
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
            Character.CharacterControllerUnityComponent.Move(new Vector3(_moveInput.x, 0f, _moveInput.y) * (speed * SpeedMultiplier * CrouchSpeedMultiplier * Time.deltaTime));
        }

        private void HandleMove(Vector2 input)
        {
            _moveInput = input;
        }
    }
}