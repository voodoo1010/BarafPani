using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Abilities.Core.Scripts
{
    public class RunnerAbilityManager : AbilityManagerBase
    {
        private const int RunnerSlotCount = 1;

        [Header("Input")]
        [SerializeField] private InputActionReference ability1Action;

        public bool HasAbility => GetSlot(0) != null;

        protected override int GetSlotCount() => RunnerSlotCount;

        private void OnEnable()
        {
            if (ability1Action == null || ability1Action.action == null) return;
            ability1Action.action.performed += HandleActivate;
            ability1Action.action.Enable();
        }

        private void OnDisable()
        {
            if (ability1Action == null || ability1Action.action == null) return;
            ability1Action.action.performed -= HandleActivate;
            ability1Action.action.Disable();
        }

        private void HandleActivate(InputAction.CallbackContext _) => Activate(0);

        /// <summary>Called by AbilityPickup. Returns true if picked up; false when the slot is occupied.</summary>
        public bool TryPickup(AbilityData data) => TryGrant(data, 0);
    }
}
