using _Features.Player._Features.Sprint.Config.Scripts;
using _Features.Player._Features.Walk.Scripts;
using _Features.Player.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Player._Features.Sprint.Scripts
{
    [RequireComponent(typeof(CharacterWalk))]
    public class CharacterSprint : CharacterFeature
    {
        [SerializeField, ForceFill, Tooltip("ScriptableObject with sprint speed multiplier")]
        private CharacterSprintSettings characterSprintSettings;
        private CharacterWalk _characterWalk;

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
            if (_characterWalk) _characterWalk.SpeedMultiplier = pressed ? characterSprintSettings.SprintMultiplier : 1f;
        }
    }
}