using System.Collections.Generic;
using CustomInspector;
using UnityEngine;

namespace _Features.Abilities.Core.Scripts
{
    [CreateAssetMenu(fileName = "AbilityPool", menuName = "BarafPani/Abilities/Ability Pool")]
    public class AbilityPool : ScriptableObject
    {
        [HorizontalLine("Hunter", 1, FixedColor.Red)]
        [SerializeField, Tooltip("Abilities available in the hunter random loadout")]
        private List<AbilityData> hunterPool = new();

        [HorizontalLine("Runner", 1, FixedColor.Green)]
        [SerializeField, Tooltip("Abilities available as runner world pickups")]
        private List<AbilityData> runnerPool = new();

        [HorizontalLine("Settings", 1, FixedColor.Cyan)]
        [SerializeField, Tooltip("If true, each roll picks unique abilities with no duplicates")]
        private bool uniqueRoll = true;

        // ─────────────────────────────────────────────────────────────
        // Debug / Testing
        // ─────────────────────────────────────────────────────────────

        [HorizontalLine("Debug / Testing", 2, FixedColor.Yellow)]
        [MessageBox(
            "Debug mode bypasses the random roll and grants exactly the abilities listed below. " +
            "Has no effect in builds — stripped at compile time.",
            MessageBoxType.Warning)]
        [SerializeField, Tooltip("Enable to override the random roll with a fixed debug loadout")]
        private bool debugMode = false;

        [SerializeField, Tooltip("Exact abilities granted when Debug Mode is on. " +
                                 "Slot 0 = first entry, Slot 1 = second, etc. " +
                                 "Null entries are skipped (slot stays empty).")]
        private List<AbilityData> debugHunterLoadout = new();

        // ─────────────────────────────────────────────────────────────
        // Public API (unchanged)
        // ─────────────────────────────────────────────────────────────

        public IReadOnlyList<AbilityData> HunterPool => hunterPool;
        public IReadOnlyList<AbilityData> RunnerPool => runnerPool;
        public bool UniqueRoll => uniqueRoll;

        /// <summary>
        /// Returns the hunter loadout to grant.
        /// In debug mode returns the fixed debug list (null entries skipped).
        /// In normal mode returns a random roll as usual.
        /// </summary>
        public List<AbilityData> RollHunter(int count, System.Random rng)
        {
#if UNITY_EDITOR
            if (debugMode)
                return BuildDebugLoadout(count);
#endif
            return Roll(hunterPool, count, rng);
        }

        // ─────────────────────────────────────────────────────────────
        // Internal
        // ─────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        /// <summary>
        /// Builds a loadout from the debug list.
        /// Null slots in the list translate to an empty ability slot.
        /// </summary>
        private List<AbilityData> BuildDebugLoadout(int count)
        {
            var result = new List<AbilityData>(count);

            for (int i = 0; i < count; i++)
            {
                // If the debug list has an entry for this slot, use it (even if null = empty slot)
                AbilityData entry = i < debugHunterLoadout.Count ? debugHunterLoadout[i] : null;
                result.Add(entry);
            }

            return result;
        }
#endif

        private List<AbilityData> Roll(List<AbilityData> source, int count, System.Random rng)
        {
            var result = new List<AbilityData>(count);
            if (source == null || source.Count == 0 || count <= 0) return result;

            if (uniqueRoll)
            {
                var bag = new List<AbilityData>(source);
                int take = Mathf.Min(count, bag.Count);
                for (int i = 0; i < take; i++)
                {
                    int idx = rng.Next(bag.Count);
                    result.Add(bag[idx]);
                    bag.RemoveAt(idx);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                    result.Add(source[rng.Next(source.Count)]);
            }

            return result;
        }
    }
}