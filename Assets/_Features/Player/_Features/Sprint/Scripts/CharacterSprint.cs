using _Features.Player._Features.Walk.Scripts;
using _Features.Player.Scripts;
using UnityEngine;

namespace _Features.Player._Features.Sprint.Scripts
{
    [RequireComponent(typeof(CharacterWalk))]
    public class CharacterSprint : CharacterFeature
    {
        [SerializeField] private float sprintMultiplier = 1.5f;
        private CharacterWalk _characterWalk;
        //TODO: put this data shit in scriptableojects

        protected override void Awake()
        {
            base.Awake();
            _characterWalk = GetComponent<CharacterWalk>();
        }
        private void OnEnable()
        {
            Character.OnSprintInput += HandleSprint;
        }
        private void OnDisable()
        {
            if (_characterWalk) _characterWalk.SpeedMultiplier = 1f;
            Character.OnSprintInput -= HandleSprint;
        }
        private void HandleSprint(bool pressed)
        {
            if (_characterWalk) _characterWalk.SpeedMultiplier = pressed ? sprintMultiplier : 1f;
        }
    }
}