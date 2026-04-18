using CustomInspector;
using UnityEngine;

namespace _Features.Abilities.Core.Scripts
{
    [CreateAssetMenu(fileName = "AbilityData", menuName = "BarafPani/Abilities/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        [HorizontalLine("Identity", 1, FixedColor.Cyan)]
        [SerializeField, Tooltip("Unique identifier for this ability")]
        private string abilityId;
        [SerializeField, Tooltip("Display name shown in UI")]
        private string displayName;
        [SerializeField, Tooltip("Ability icon for HUD and menus")]
        private Sprite icon;
        [SerializeField, Tooltip("Tint color for UI elements")]
        private Color uiColor = Color.white;

        [HorizontalLine("Prefab", 1, FixedColor.Cyan)]
        [Tooltip("Prefab whose root has an AbilityBase-derived component. Instantiated under the character on acquire.")]
        [SerializeField, ForceFill] private AbilityBase abilityPrefab;

        [HorizontalLine("Rules", 1, FixedColor.Orange)]
        [SerializeField, Tooltip("Seconds between activations"), Range(0f, 30f)]
        private float cooldown = 5f;
        [SerializeField, Tooltip("Number of uses before recharge or removal"), Range(1, 10)]
        private int maxCharges = 1;
        [Tooltip("If true, ability is removed from the slot when charges reach zero. Runner pickups typically set this true with maxCharges = 1.")]
        [SerializeField] private bool consumeOnEmpty = false;

        [HorizontalLine("Pool Eligibility", 1, FixedColor.Green)]
        [SerializeField, Tooltip("Include in hunter random loadout pool")]
        private bool hunterPool = true;
        [SerializeField, Tooltip("Include in runner pickup pool")]
        private bool runnerPool = false;

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
