using _Features.Abilities.Core.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Abilities.Hunter
{
    public class HookController : AbilityBase
    {
        [Header("Hook Settings")]
        [SerializeField, Tooltip("Speed of the hook projectile in units/s"), Range(5f, 50f)]
        private float _hookSpeed = 20f;
        [SerializeField, Tooltip("Speed at which caught target is pulled in"), Range(5f, 50f)]
        private float _pullSpeed = 15f;
        [SerializeField, Tooltip("Maximum travel distance before the hook retracts"), Range(10f, 100f)]
        private float _maxHookDistance = 30f;

        [Header("Layer")]
        [SerializeField, Tooltip("Layers that the hook can latch onto")]
        private LayerMask _runnerLayer;

        [Header("Prefabs")]
        [SerializeField, ForceFill, Tooltip("Hook projectile prefab with HookProjectile component")]
        private HookProjectile _hookPrefab;

        private Transform _hookOrigin;
        private Camera _fpsCamera;
        private HookProjectile _activeHook;

        protected override void OnAcquiredInternal()
        {
            _hookOrigin = Manager.AbilityOrigin;
            _fpsCamera = Manager.AbilityCamera;
        }

        protected override bool OnActivateInternal()
        {
            if (_activeHook != null) return false;
            if (_hookPrefab == null)
            {
                Debug.LogError("HookController: _hookPrefab is not assigned.");
                return false;
            }
            if (_fpsCamera == null || _hookOrigin == null)
            {
                Debug.LogError("HookController: camera or origin not assigned.");
                return false;
            }

            FireHook();
            ConsumeActivation();
            return true;
        }

        private void FireHook()
        {
            Ray aimRay = _fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            Vector3 targetPoint = Physics.Raycast(aimRay, out RaycastHit hit, _maxHookDistance)
                ? hit.point
                : aimRay.origin + aimRay.direction * _maxHookDistance;

            Vector3 aimDirection = (targetPoint - _hookOrigin.position).normalized;

            HookProjectile hookInstance = Instantiate(_hookPrefab, _hookOrigin.position, Quaternion.identity);
            _activeHook = hookInstance;

            _activeHook.Initialize(
                origin: _hookOrigin,
                direction: aimDirection,
                hookSpeed: _hookSpeed,
                pullSpeed: _pullSpeed,
                maxDistance: _maxHookDistance,
                runnerLayer: _runnerLayer,
                onFinished: OnHookFinished
            );
        }

        private void OnHookFinished()
        {
            _activeHook = null;
        }
    }
}