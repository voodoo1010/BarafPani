using System;
using _Features.Player.Scripts;
using UnityEngine;

namespace _Features.Abilities.Core.Scripts
{
    public abstract class AbilityManagerBase : CharacterFeature
    {
        [SerializeField] protected Transform abilityParent;
        [SerializeField] private Transform abilityOrigin;
        [SerializeField] private Camera abilityCamera;

        protected AbilityBase[] Slots;

        public int SlotCount => Slots?.Length ?? 0;
        public Transform AbilityOrigin => abilityOrigin;
        public Camera AbilityCamera => abilityCamera;
        public AbilityBase GetSlot(int index) => (index >= 0 && index < SlotCount) ? Slots[index] : null;

        public event Action<int, AbilityBase> OnSlotChanged;
        public event Action<int, AbilityBase> OnAbilityActivated;
        public event Action<int, AbilityBase> OnAbilityExpired;

        protected override void Awake()
        {
            base.Awake();
            Slots = new AbilityBase[GetSlotCount()];
            if (abilityParent == null) abilityParent = transform;
        }

        protected abstract int GetSlotCount();

        public bool TryGrant(AbilityData data, int slot)
        {
            if (data == null) return false;
            if (slot < 0 || slot >= SlotCount) return false;
            if (Slots[slot] != null) return false;
            if (data.AbilityPrefab == null)
            {
                Debug.LogError($"[AbilityManager] AbilityData '{data.DisplayName}' has no prefab assigned.");
                return false;
            }

            var instance = Instantiate(data.AbilityPrefab, abilityParent);
            instance.OnAcquired(Character, data, this);
            instance.OnConsumed += HandleAbilityConsumed;
            Slots[slot] = instance;
            OnSlotChanged?.Invoke(slot, instance);
            return true;
        }

        public void Clear(int slot)
        {
            if (slot < 0 || slot >= SlotCount) return;
            var ability = Slots[slot];
            if (ability == null) return;

            ability.OnConsumed -= HandleAbilityConsumed;
            ability.OnReleased();
            Slots[slot] = null;
            Destroy(ability.gameObject);
            OnSlotChanged?.Invoke(slot, null);
        }

        public bool Activate(int slot)
        {
            if (slot < 0 || slot >= SlotCount) return false;
            var ability = Slots[slot];
            if (ability == null) return false;
            if (!ability.TryActivate()) return false;

            OnAbilityActivated?.Invoke(slot, ability);
            return true;
        }

        private void HandleAbilityConsumed(AbilityBase ability)
        {
            int slot = IndexOf(ability);
            if (slot < 0) return;

            if (ability.Data != null && ability.Data.ConsumeOnEmpty && ability.ChargesRemaining <= 0)
            {
                OnAbilityExpired?.Invoke(slot, ability);
                Clear(slot);
            }
        }

        private int IndexOf(AbilityBase ability)
        {
            for (int i = 0; i < SlotCount; i++)
                if (Slots[i] == ability) return i;
            return -1;
        }

        public int FindEmptySlot()
        {
            for (int i = 0; i < SlotCount; i++)
                if (Slots[i] == null) return i;
            return -1;
        }
    }
}
