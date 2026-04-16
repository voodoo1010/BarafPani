using System.Collections;
using _Features.Abilities.Core.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Abilities.Hunter
{
    public class BearTrapAbility : AbilityBase
    {
        [Header("Prefabs")]
        [Tooltip("The actual bear trap prefab. Must have BearTrapObject.cs + a Trigger Collider.")]
        public GameObject TrapPrefab;

        [Tooltip("Ghost/preview prefab shown during placement (transparent, no BearTrapObject script).")]
        public GameObject TrapGhostPrefab;

        [Header("Placement")]
        public float MaxPlaceDistance = 8f;
        public float GroundOffset = 0.05f;

        [Header("Trap Limit")]
        [Tooltip("Maximum number of traps that can exist at once. 0 = unlimited.")]
        public int MaxTrapsActive = 3;

        [Header("Layer Mask")]
        public LayerMask GroundLayer;

        [Header("UI (Optional)")]
        public UnityEngine.UI.Image CooldownRadialUI;

        private bool _isPlacing;
        private GameObject _ghostInstance;
        private Camera _camera;
        private int _activeTraps;

        private static readonly Color ColourValid = new(0.7f, 0.45f, 0.1f, 0.55f);
        private static readonly Color ColourInvalid = new(1f, 0.15f, 0.15f, 0.55f);

        private InputAction _placeAction;
        private InputAction _cancelAction;

        protected override void OnAcquiredInternal()
        {
            _camera = Camera.main;

            _placeAction = new InputAction("TrapPlace", binding: "<Mouse>/leftButton");
            _placeAction.performed += _ => HandlePlace();

            _cancelAction = new InputAction("TrapCancel", binding: "<Mouse>/rightButton");
            _cancelAction.performed += _ =>
            {
                if (_isPlacing) CancelPlacement();
            };

            _placeAction.Enable();
            _cancelAction.Enable();

            if (TrapPrefab == null) Debug.LogError("[BearTrap] TrapPrefab is not assigned.");
            if (TrapGhostPrefab == null) Debug.LogError("[BearTrap] TrapGhostPrefab is not assigned.");
        }

        protected override void OnReleasedInternal()
        {
            if (_isPlacing) CancelPlacement();
            _placeAction?.Dispose();
            _cancelAction?.Dispose();
            _placeAction = null;
            _cancelAction = null;
        }

        protected override void Update()
        {
            base.Update();
            UpdateRadialUI();

            if (_isPlacing) UpdateGhost();
        }

        protected override bool OnActivateInternal()
        {
            if (MaxTrapsActive > 0 && _activeTraps >= MaxTrapsActive)
            {
                Debug.Log($"[BearTrap] Trap limit reached ({MaxTrapsActive}).");
                return false;
            }

            if (_isPlacing) CancelPlacement();
            else EnterPlacementMode();

            return true;
        }

        private void UpdateRadialUI()
        {
            if (CooldownRadialUI != null)
                CooldownRadialUI.fillAmount = CooldownNormalized;
        }

        private void EnterPlacementMode()
        {
            _isPlacing = true;

            if (TrapGhostPrefab == null) return;

            _ghostInstance = Instantiate(TrapGhostPrefab);
            _ghostInstance.name = "BearTrapGhost";
            DisableAllColliders(_ghostInstance);
        }

        private void CancelPlacement()
        {
            _isPlacing = false;
            DestroyGhost();
        }

        private void UpdateGhost()
        {
            if (_ghostInstance == null) return;

            bool hit = GetGroundPoint(out Vector3 point, out bool isValid);

            if (!hit)
            {
                _ghostInstance.SetActive(false);
                return;
            }

            _ghostInstance.SetActive(true);
            _ghostInstance.transform.position = point;
            _ghostInstance.transform.rotation = Quaternion.identity;

            SetGhostColour(isValid ? ColourValid : ColourInvalid);
        }

        private void HandlePlace()
        {
            if (!_isPlacing) return;

            bool hit = GetGroundPoint(out Vector3 point, out bool isValid);

            if (hit && isValid) SpawnTrap(point);
            else Debug.Log("[BearTrap] Invalid placement.");
        }

        private void SpawnTrap(Vector3 position)
        {
            if (TrapPrefab == null) return;

            GameObject trap = Instantiate(TrapPrefab, position, Quaternion.identity);
            trap.name = "BearTrap";
            _activeTraps++;

            if (trap.GetComponent<BearTrapObject>() != null)
                StartCoroutine(TrackTrapLifetime(trap));

            _isPlacing = false;
            DestroyGhost();
            ConsumeActivation();
        }

        private IEnumerator TrackTrapLifetime(GameObject trap)
        {
            while (trap != null) yield return null;
            _activeTraps = Mathf.Max(0, _activeTraps - 1);
        }

        private bool GetGroundPoint(out Vector3 worldPoint, out bool isValid)
        {
            worldPoint = Vector3.zero;
            isValid = false;

            if (_camera == null || Mouse.current == null) return false;

            Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GroundLayer))
                return false;

            worldPoint = hit.point + Vector3.up * GroundOffset;
            isValid = Vector3.Distance(transform.position, hit.point) <= MaxPlaceDistance;
            return true;
        }

        private void DestroyGhost()
        {
            if (_ghostInstance == null) return;
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }

        private void SetGhostColour(Color colour)
        {
            if (_ghostInstance == null) return;

            foreach (Renderer r in _ghostInstance.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in r.materials)
                {
                    if (mat.HasProperty("_Color")) mat.color = colour;
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", colour);
                }
            }
        }

        private void DisableAllColliders(GameObject go)
        {
            foreach (Collider col in go.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, MaxPlaceDistance);
        }
    }
}
