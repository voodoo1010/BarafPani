using System;
using _Features.Player.Scripts;
using UnityEngine;

namespace _Features.Abilities.Core.Scripts
{
    public abstract class AbilityBase : MonoBehaviour, IAbility
    {
        protected Character Owner { get; private set; }
        protected AbilityManagerBase Manager { get; private set; }

        public AbilityData Data { get; private set; }
        public int ChargesRemaining { get; private set; }
        public float CooldownRemaining { get; private set; }

        public float CooldownNormalized =>
            Data == null || Data.Cooldown <= 0f ? 0f : Mathf.Clamp01(CooldownRemaining / Data.Cooldown);

        public bool IsReady => CooldownRemaining <= 0f && ChargesRemaining > 0;

        /// <summary>Fired after a successful ConsumeActivation (charge spent, cooldown set).</summary>
        public event Action<AbilityBase> OnConsumed;

        public virtual void OnAcquired(Character owner, AbilityData data, AbilityManagerBase manager)
        {
            Owner = owner;
            Data = data;
            Manager = manager;
            ChargesRemaining = data.MaxCharges;
            CooldownRemaining = 0f;
            OnAcquiredInternal();
        }

        public virtual void OnReleased()
        {
            OnReleasedInternal();
            Owner = null;
            Data = null;
            Manager = null;
        }

        public bool TryActivate()
        {
            if (!IsReady) return false;
            return OnActivateInternal();
        }

        /// <summary>
        /// Subclasses call this when the ability actually fires — decrements a charge and starts cooldown.
        /// Instant abilities call it inside OnActivateInternal. Multi-step abilities (placement flows)
        /// call it at the moment the action commits.
        /// </summary>
        protected void ConsumeActivation()
        {
            if (Data == null) return;
            ChargesRemaining = Mathf.Max(0, ChargesRemaining - 1);
            CooldownRemaining = Data.Cooldown;
            OnConsumed?.Invoke(this);
        }

        protected virtual void Update()
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Time.deltaTime);
            }
        }

        protected virtual void OnAcquiredInternal() { }
        protected virtual void OnReleasedInternal() { }

        /// <summary>
        /// Start or perform the ability. Return true if the input was accepted (e.g. entered placement
        /// mode or instantly fired). Return false to reject. Call ConsumeActivation() when the ability
        /// actually commits (may be same frame or later).
        /// </summary>
        protected abstract bool OnActivateInternal();
    }
}
