using UnityEngine;

namespace _Features.Abilities.Core.Scripts
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "BarafPani/Abilities/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string abilityId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color uiColor = Color.white;

        [Header("Prefab")]
        [Tooltip("Prefab whose root has an AbilityBase-derived component. Instantiated under the character on acquire.")]
        [SerializeField] private AbilityBase abilityPrefab;

        [Header("Rules")]
        [Min(0f)] [SerializeField] private float cooldown = 5f;
        [Min(1)]  [SerializeField] private int maxCharges = 1;
        [Tooltip("If true, ability is removed from the slot when charges reach zero. Runner pickups typically set this true with maxCharges = 1.")]
        [SerializeField] private bool consumeOnEmpty = false;

        [Header("Pool Eligibility")]
        [SerializeField] private bool hunterPool = true;
        [SerializeField] private bool runnerPool = false;

        public string AbilityId => string.IsNullOrEmpty(abilityId) ? name : abilityId;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public Sprite Icon => icon;
        public Color UiColor => uiColor;
        public AbilityBase AbilityPrefab => abilityPrefab;
        public float Cooldown => cooldown;
        public int MaxCharges => maxCharges;
        public bool ConsumeOnEmpty => consumeOnEmpty;
        public bool InHunterPool => hunterPool;
        public bool InRunnerPool => runnerPool;
    }
}
