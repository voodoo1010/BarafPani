using System.Collections.Generic;
using UnityEngine;

namespace _Features.Abilities.Core.Scripts
{
    [CreateAssetMenu(fileName = "AbilityPool", menuName = "BarafPani/Abilities/Ability Pool")]
    public class AbilityPool : ScriptableObject
    {
        [SerializeField] private List<AbilityData> hunterPool = new();
        [SerializeField] private List<AbilityData> runnerPool = new();
        [SerializeField] private bool uniqueRoll = true;

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
