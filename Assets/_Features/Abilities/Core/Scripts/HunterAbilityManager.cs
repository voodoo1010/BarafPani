using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Abilities.Core.Scripts
{
    public class HunterAbilityManager : AbilityManagerBase
    {
        private const int HunterSlotCount = 2;

        [Header("Pool")]
        [SerializeField] private AbilityPool pool;
        [SerializeField] private bool rollOnAwake = false;
        [SerializeField] private int rollSeed = -1;

        [Header("Input")]
        [SerializeField] private InputActionReference ability1Action;
        [SerializeField] private InputActionReference ability2Action;

        protected override int GetSlotCount() => HunterSlotCount;

        protected override void Awake()
        {
            base.Awake();
            if (rollOnAwake) RollLoadout(rollSeed);
        }

        private void OnEnable()
        {
            BindAction(ability1Action, 0, true);
            BindAction(ability2Action, 1, true);
        }

        private void OnDisable()
        {
            BindAction(ability1Action, 0, false);
            BindAction(ability2Action, 1, false);
        }

        private void BindAction(InputActionReference reference, int slot, bool bind)
        {
            if (reference == null || reference.action == null) return;
            if (bind)
            {
                reference.action.performed += ctx => Activate(slot);
                reference.action.Enable();
            }
            else
            {
                reference.action.Disable();
            }
        }

        [ContextMenu("Roll Loadout")]
        public void RollLoadout() => RollLoadout(rollSeed);

        public void RollLoadout(int seed)
        {
            if (pool == null)
            {
                Debug.LogError("[HunterAbilityManager] No AbilityPool assigned.");
                return;
            }

            for (int i = 0; i < SlotCount; i++) Clear(i);

            var rng = seed < 0 ? new System.Random() : new System.Random(seed);
            var rolled = pool.RollHunter(SlotCount, rng);
            for (int i = 0; i < rolled.Count && i < SlotCount; i++)
            {
                TryGrant(rolled[i], i);
            }
        }
    }
}
