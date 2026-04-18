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

        public IReadOnlyList<AbilityData> HunterPool => hunterPool;
        public IReadOnlyList<AbilityData> RunnerPool => runnerPool;
        public bool UniqueRoll => uniqueRoll;

        public List<AbilityData> RollHunter(int count, System.Random rng)
        {
            return Roll(hunterPool, count, rng);
        }

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
                {
                    result.Add(source[rng.Next(source.Count)]);
                }
            }
            return result;
        }
    }
}
