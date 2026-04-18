using _Features.Hunter.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Abilities.Core.Scripts
{
    public class HunterAbilities : AbilityManagerBase
    {
        private const int HunterSlotCount = 2;

        [HorizontalLine("Loadout", 1, FixedColor.Orange)]
        [SerializeField, ForceFill, Tooltip("ScriptableObject containing the ability pool for random loadout rolls")]
        private AbilityPool pool;
        [SerializeField, Tooltip("Automatically roll a random loadout on Awake")]
        private bool rollOnAwake = false;
        [SerializeField, Tooltip("RNG seed for reproducible rolls. Negative = random seed.")]
        private int rollSeed = -1;

        private HunterCharacterInput _hunterInput;

        protected override int GetSlotCount() => HunterSlotCount;

        protected override void Awake()
        {
            base.Awake();
            _hunterInput = GetComponent<HunterCharacterInput>();
            if (rollOnAwake) RollLoadout(rollSeed);
        }

        private void OnEnable()
        {
            _hunterInput.OnAbilityInput += HandleAbilityInput;
        }

        private void OnDisable()
        {
            _hunterInput.OnAbilityInput -= HandleAbilityInput;
        }

        private void HandleAbilityInput(int slot)
        {
            Activate(slot);
        }

        [ContextMenu("Roll Loadout")]
        public void RollLoadout() => RollLoadout(rollSeed);

        public void RollLoadout(int seed)
        {
            if (pool == null)
            {
                Debug.LogError("[HunterAbilities] No AbilityPool assigned.");
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
