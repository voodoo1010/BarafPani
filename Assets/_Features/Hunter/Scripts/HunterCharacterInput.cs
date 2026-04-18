using System;
using _Features.Player._Features.Input;
using CustomInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Hunter.Scripts
{
    public class HunterCharacterInput : CharacterInput
    {
        [HorizontalLine("Hunter Input", 1, FixedColor.Red)]
        [SerializeField, ForceFill, Tooltip("Primary attack input (LMB)")]
        private InputActionReference attackAction;
        [SerializeField, Tooltip("Ability slot 1 input")]
        private InputActionReference ability1Action;
        [SerializeField, Tooltip("Ability slot 2 input")]
        private InputActionReference ability2Action;

        public event Action OnAttackInput;
        public event Action<int> OnAbilityInput;

        protected override void OnEnable()
        {
            base.OnEnable();
            BindAction(attackAction, OnAttack, true);
            BindAction(ability1Action, OnAbility1, true);
            BindAction(ability2Action, OnAbility2, true);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            BindAction(attackAction, OnAttack, false);
            BindAction(ability1Action, OnAbility1, false);
            BindAction(ability2Action, OnAbility2, false);
        }

        private void BindAction(InputActionReference actionRef, Action<InputAction.CallbackContext> callback, bool bind)
        {
            if (actionRef == null || actionRef.action == null) return;
            if (bind)
            {
                actionRef.action.performed += callback;
                actionRef.action.Enable();
            }
            else
            {
                actionRef.action.performed -= callback;
                actionRef.action.Disable();
            }
        }

        private void OnAttack(InputAction.CallbackContext ctx) => OnAttackInput?.Invoke();
        private void OnAbility1(InputAction.CallbackContext ctx) => OnAbilityInput?.Invoke(0);
        private void OnAbility2(InputAction.CallbackContext ctx) => OnAbilityInput?.Invoke(1);
    }
}