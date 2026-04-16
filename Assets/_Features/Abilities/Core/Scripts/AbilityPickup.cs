using UnityEngine;

namespace _Features.Abilities.Core.Scripts
{
    [RequireComponent(typeof(Collider))]
    public class AbilityPickup : MonoBehaviour
    {
        [SerializeField] private AbilityData abilityData;
        [Tooltip("Destroys the pickup GameObject on a successful grant. If false, caller can handle despawn externally.")]
        [SerializeField] private bool destroyOnPickup = true;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (abilityData == null) return;

            var runner = other.GetComponentInParent<RunnerAbilityManager>();
            if (runner == null) return;

            if (runner.TryPickup(abilityData) && destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
