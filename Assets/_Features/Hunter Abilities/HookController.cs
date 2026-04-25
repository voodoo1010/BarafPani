using _Features.Abilities.Core.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Abilities.Hunter
{
    public class HookController : AbilityBase
    {
        [Header("Hook Settings")]
        [SerializeField, Range(5f, 50f)] private float _hookSpeed = 20f;
        [SerializeField, Range(5f, 50f)] private float _pullSpeed = 15f;
        [SerializeField, Range(10f, 100f)] private float _maxHookDistance = 30f;

        [Header("Layer")]
        [SerializeField] private LayerMask _runnerLayer;

        [Header("Prefabs")]
        [SerializeField, ForceFill] private HookProjectile _hookPrefab;

        [Header("Visuals")]
        [SerializeField, Tooltip("Name of the Hook GameObject in the character rig")]
        private string _hookVisualName = "Hook";

        [SerializeField, Tooltip("Name of the Rope GameObject in the character rig")]
        private string _ropeVisualName = "Rope";
        private GameObject _hookVisual;
        private GameObject _ropeVisual;

        private Transform _hookOrigin;
        private Camera _fpsCamera;
        private HookProjectile _activeHook;
        private bool _isArmed;

        protected override void OnAcquiredInternal()
        {
            _hookOrigin = Manager.AbilityOrigin;
            _fpsCamera = Manager.AbilityCamera;

            _hookVisual = FindInOwner(_hookVisualName);
            _ropeVisual = FindInOwner(_ropeVisualName);

            SetVisualsActive(false);
        }

        private GameObject FindInOwner(string goName)
        {
            Transform[] all = Owner.transform.root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
                if (t.name == goName) return t.gameObject;

            Debug.LogWarning($"HookController: Could not find '{goName}' in character hierarchy.");
            return null;
        }
        protected override bool OnActivateInternal()
        {
            // show visuals, wait for attack input
            if (_isArmed || _activeHook != null) return false;
            _isArmed = true;
            SetVisualsActive(true);
            return true;
        }

        public override bool OnAbilityAttackInternal()
        {
            if (!_isArmed || _activeHook != null) return false;
            if (_hookPrefab == null || _fpsCamera == null || _hookOrigin == null)
            {
                Debug.LogError("HookController: missing references.");
                return false;
            }

            _isArmed = false;
            FireHook();
            ConsumeActivation();
            return true;
        }

        public override void OnCancelInternal()
        {
            _isArmed = false;
            SetVisualsActive(false);
        }

        private void FireHook()
        {
            Ray aimRay = _fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetPoint = Physics.Raycast(aimRay, out RaycastHit hit, _maxHookDistance, _runnerLayer)
                ? hit.point
                : aimRay.origin + aimRay.direction * _maxHookDistance;

            _activeHook = Instantiate(_hookPrefab, _hookOrigin.position, Quaternion.identity);
            _activeHook.Initialize(_hookOrigin, (targetPoint - _hookOrigin.position).normalized,
                _hookSpeed, _pullSpeed, _maxHookDistance, _runnerLayer, OnHookFinished);
        }

        private void OnHookFinished()
        {
            _activeHook = null;
            SetVisualsActive(false);
        }

        private void SetVisualsActive(bool active)
        {
            if (_hookVisual != null) _hookVisual.SetActive(active);
            if (_ropeVisual != null) _ropeVisual.SetActive(active);
        }
    }
}