using System.Collections;
using _Features.Abilities.Core.Scripts;
using CustomInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Abilities.Hunter
{
    public class BearTrapAbility : AbilityBase
    {
        [Header("Prefabs")]
        [Tooltip("The actual bear trap prefab. Must have BearTrapObject.cs + a Trigger Collider."), ForceFill]
        public GameObject TrapPrefab;

        [Tooltip("Ghost/preview prefab shown during placement (transparent, no BearTrapObject script)."), ForceFill]
        public GameObject TrapGhostPrefab;

        [Header("Aim & Distance")]
        [Range(1f, 5f), Tooltip("Minimum placement distance (when looking down)")]
        public float MinDistance = 1.5f;

        [Range(3f, 30f), Tooltip("Maximum placement distance (when looking forward)")]
        public float MaxDistance = 8f;

        [Range(0f, 1f), Tooltip("Vertical offset above ground")]
        public float GroundOffset = 0.05f;

        [Header("Rotation (Optional)")]
        [Range(5f, 90f), Tooltip("Degrees per scroll tick")]
        public float RotationStep = 15f;

        [Header("Trap Limit")]
        [Range(0, 10), Tooltip("Maximum active traps. 0 = unlimited.")]
        public int MaxTrapsActive = 3;

        [Header("Layer Mask")]
        [Tooltip("Layers considered valid ground")]
        public LayerMask GroundLayer = ~0;

        [Header("Validation Colors")]
        public Color ValidColor = new(0.7f, 0.45f, 0.1f, 0.55f);
        public Color InvalidColor = new(1f, 0.15f, 0.15f, 0.55f);

        [Header("UI (Optional)")]
        public UnityEngine.UI.Image CooldownRadialUI;

        private bool _isPlacing;
        private float _yawOffset;
        private GameObject _ghostInstance;
        private Camera _camera;
        private int _activeTraps;

        private InputAction _scrollAction;

        protected override void OnAcquiredInternal()
        {
            _camera = Camera.main;
            if (_camera == null)
                Debug.LogError("[BearTrap] No Main Camera found! Tag your camera as MainCamera.");

            _scrollAction = new InputAction("TrapScroll", binding: "<Mouse>/scroll/y");
            _scrollAction.Enable();

            if (TrapPrefab == null) Debug.LogError("[BearTrap] TrapPrefab is not assigned.");
            if (TrapGhostPrefab == null) Debug.LogError("[BearTrap] TrapGhostPrefab is not assigned.");
        }

        protected override void OnReleasedInternal()
        {
            if (_isPlacing) CancelPlacement();
            _scrollAction?.Dispose();
            _scrollAction = null;
        }

        protected override void Update()
        {
            base.Update();

            if (CooldownRadialUI != null)
                CooldownRadialUI.fillAmount = CooldownNormalized;

            if (_isPlacing)
            {
                HandleScrollRotation();
                UpdateGhost();
            }
        }

        protected override bool OnActivateInternal()
        {
            if (MaxTrapsActive > 0 && _activeTraps >= MaxTrapsActive)
            {
                Debug.Log($"[BearTrap] Trap limit reached ({MaxTrapsActive}).");
                return false;
            }

            if (!_isPlacing)
                EnterPlacementMode();
            else
                ConfirmPlacement();

            return true;
        }

        // ───────────────────────────────────────────────────────────
        // Preview Lifecycle
        // ───────────────────────────────────────────────────────────

        private void EnterPlacementMode()
        {
            if (TrapGhostPrefab == null) return;

            _isPlacing = true;
            _yawOffset = 0f;

            _ghostInstance = Instantiate(TrapGhostPrefab);
            _ghostInstance.name = "BearTrapGhost";
            DisableAllColliders(_ghostInstance);

            UpdateGhost();
        }

        private void CancelPlacement()
        {
            _isPlacing = false;
            DestroyGhost();
        }

        private void ConfirmPlacement()
        {
            if (TrapPrefab == null) return;
            if (!TryGetPlacement(out Vector3 point, out Quaternion rotation, out bool isValid)) return;

            if (!isValid)
            {
                Debug.Log("[BearTrap] Invalid placement.");
                return;
            }

            GameObject trap = Instantiate(TrapPrefab, point, rotation);
            trap.name = "BearTrap";
            _activeTraps++;

            if (trap.GetComponent<BearTrapObject>() != null)
                StartCoroutine(TrackTrapLifetime(trap));

            CancelPlacement();
            ConsumeActivation();
        }

        // ───────────────────────────────────────────────────────────
        // Input
        // ───────────────────────────────────────────────────────────

        private void HandleScrollRotation()
        {
            if (_scrollAction == null) return;

            float scroll = _scrollAction.ReadValue<float>();
            if (Mathf.Abs(scroll) <= 0.01f) return;

            _yawOffset += scroll > 0 ? RotationStep : -RotationStep;
            _yawOffset = Mathf.Repeat(_yawOffset, 360f);
        }

        // ───────────────────────────────────────────────────────────
        // Aim Calculation (Sage-style: screen center ray)
        // ───────────────────────────────────────────────────────────

        private bool TryGetPlacement(out Vector3 worldPoint, out Quaternion rotation, out bool isValid)
        {
            worldPoint = Vector3.zero;
            rotation = Quaternion.identity;
            isValid = false;

            if (_camera == null) return false;

            // Ray from camera through screen center
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            Vector3 aimPoint;
            Vector3 surfaceNormal = Vector3.up;

            // Try to hit ground
            if (Physics.Raycast(ray, out RaycastHit hit, MaxDistance * 2f, GroundLayer))
            {
                aimPoint = hit.point;
                surfaceNormal = hit.normal;
            }
            else
            {
                // Fallback: project ray onto horizontal plane at player feet
                Plane groundPlane = new Plane(Vector3.up, transform.position);
                if (groundPlane.Raycast(ray, out float dist))
                    aimPoint = ray.GetPoint(dist);
                else
                    return false;
            }

            // Clamp distance from player
            Vector3 toAim = aimPoint - transform.position;
            toAim.y = 0f;
            float distance = toAim.magnitude;
            distance = Mathf.Clamp(distance, MinDistance, MaxDistance);

            Vector3 horizontalDir = toAim.sqrMagnitude > 0.001f ? toAim.normalized : transform.forward;

            // Final placement point — at original aim height if hit was valid, else flat
            Vector3 basePos = transform.position + horizontalDir * distance;
            basePos.y = aimPoint.y;
            worldPoint = basePos + Vector3.up * GroundOffset;

            // Rotation: align trap "up" with surface normal, face away from player, plus scroll offset
            Vector3 trapForward = horizontalDir;
            rotation = Quaternion.LookRotation(trapForward, surfaceNormal) * Quaternion.Euler(0f, _yawOffset, 0f);

            // Valid if the actual aim distance was within range (didn't have to clamp too hard)
            isValid = (aimPoint - transform.position).magnitude <= MaxDistance * 1.05f;

            return true;
        }

        // ───────────────────────────────────────────────────────────
        // Ghost Update
        // ───────────────────────────────────────────────────────────

        private void UpdateGhost()
        {
            if (_ghostInstance == null) return;

            if (!TryGetPlacement(out Vector3 point, out Quaternion rotation, out bool isValid))
            {
                _ghostInstance.SetActive(false);
                return;
            }

            _ghostInstance.SetActive(true);
            _ghostInstance.transform.position = point;
            _ghostInstance.transform.rotation = rotation;

            SetGhostColour(isValid ? ValidColor : InvalidColor);
        }

        private IEnumerator TrackTrapLifetime(GameObject trap)
        {
            while (trap != null) yield return null;
            _activeTraps = Mathf.Max(0, _activeTraps - 1);
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
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", colour);
                    else if (mat.HasProperty("_Color")) mat.color = colour;
                }
            }
        }

        private void DisableAllColliders(GameObject go)
        {
            foreach (Collider col in go.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }

        private void OnDisable()
        {
            DestroyGhost();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, MaxDistance);
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, MinDistance);
        }
    }
}