using System.Collections;
using _Features.Abilities.Core.Scripts;
using CustomInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Abilities.Hunter
{
    public class BearTrapAbility : AbilityBase
    {
        // ─────────────────────────────────────────────────────────────
        // Inspector
        // ─────────────────────────────────────────────────────────────

        [Header("Prefabs")]
        [Tooltip("The actual bear trap prefab. Must have BearTrapObject.cs + a Trigger Collider."), ForceFill]
        public GameObject TrapPrefab;

        [Tooltip("Ghost/preview prefab shown during placement (transparent, no BearTrapObject script)."), ForceFill]
        public GameObject TrapGhostPrefab;

        [Header("Aim & Distance")]
        [Range(1f, 5f), Tooltip("Minimum placement distance from origin")]
        public float MinDistance = 1.5f;

        [Range(3f, 30f), Tooltip("Maximum placement distance from origin")]
        public float MaxDistance = 8f;

        [Range(0f, 1f), Tooltip("Vertical offset above the snapped ground point")]
        public float GroundOffset = 0.05f;

        [Header("Ground Snap")]
        [Tooltip("How far above the clamped XZ point the snap ray starts")]
        public float SnapRayOriginHeight = 10f;

        [Tooltip("Maximum downward distance the snap ray travels")]
        public float SnapRayDistance = 20f;

        [Header("Rotation (Optional)")]
        [Range(5f, 90f), Tooltip("Degrees per scroll tick")]
        public float RotationStep = 15f;

        [Header("Trap Limit")]
        [Range(0, 10), Tooltip("Maximum active traps at once. 0 = unlimited.")]
        public int MaxTrapsActive = 3;

        [Header("Layer Masks")]
        [Tooltip("Layers considered valid ground")]
        public LayerMask GroundLayer = ~0;

        [Tooltip("Layers that block placement. Set to Nothing to skip check.")]
        public LayerMask ObstacleLayer;

        [Tooltip("Half-extents of the overlap box used to detect obstructions at the trap position")]
        public Vector3 OverlapHalfExtents = new(0.3f, 0.1f, 0.3f);

        [Header("Validation Colors")]
        public Color ValidColor = new(0.7f, 0.45f, 0.1f, 0.55f);
        public Color InvalidColor = new(1f, 0.15f, 0.15f, 0.55f);

        [Header("UI (Optional)")]
        public UnityEngine.UI.Image CooldownRadialUI;

        // ─────────────────────────────────────────────────────────────
        // Private state
        // ─────────────────────────────────────────────────────────────

        private bool _isPlacing;
        private float _yawOffset;
        private GameObject _ghostInstance;
        private Renderer[] _ghostRenderers;
        private bool _lastPlacementValid;
        private int _activeTraps;

        private Camera _cam;
        private InputAction _scrollAction;

        private MaterialPropertyBlock _mpb;
        private static readonly int ColorPropID = Shader.PropertyToID("_Color");

        // ─────────────────────────────────────────────────────────────
        // AbilityBase lifecycle
        // ─────────────────────────────────────────────────────────────

        protected override void OnAcquiredInternal()
        {
            _cam = Manager.AbilityCamera != null ? Manager.AbilityCamera : Camera.main;
            _mpb = new MaterialPropertyBlock();

            _scrollAction = new InputAction("TrapScroll", binding: "<Mouse>/scroll/y");
            _scrollAction.Enable();

            if (TrapPrefab == null) Debug.LogError("[BearTrap] TrapPrefab is not assigned.");
            if (TrapGhostPrefab == null) Debug.LogError("[BearTrap] TrapGhostPrefab is not assigned.");
        }

        protected override void OnReleasedInternal()
        {
            DestroyGhost();
            _scrollAction?.Dispose();
            _scrollAction = null;
        }

        // ─────────────────────────────────────────────────────────────
        // Phase 1 — Activate
        // ─────────────────────────────────────────────────────────────

        protected override bool OnActivateInternal()
        {
            if (MaxTrapsActive > 0 && _activeTraps >= MaxTrapsActive)
            {
                Debug.Log($"[BearTrap] Trap limit reached ({MaxTrapsActive}).");
                return false;
            }

            SpawnGhost();
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Phase 2 — Ability Attack
        // ─────────────────────────────────────────────────────────────

        public override bool OnAbilityAttackInternal()
        {
            if (!_isPlacing) return false;
            if (!TryGetPlacement(out Vector3 point, out Quaternion rotation, out bool isValid)) return false;

            if (!isValid)
            {
                Debug.Log("[BearTrap] Placement blocked — obstruction or out of range.");
                return false;
            }

            PlaceTrap(point, rotation);
            DestroyGhost();
            ConsumeActivation();
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Cancel
        // ─────────────────────────────────────────────────────────────

        public override void OnCancelInternal()
        {
            DestroyGhost();
        }

        // ─────────────────────────────────────────────────────────────
        // Update
        // ─────────────────────────────────────────────────────────────

        protected override void Update()
        {
            base.Update();

            if (CooldownRadialUI != null)
                CooldownRadialUI.fillAmount = CooldownNormalized;

            if (!_isPlacing) return;

            ReadScrollInput();
            UpdateGhost();
        }

        // ─────────────────────────────────────────────────────────────
        // Ghost management
        // ─────────────────────────────────────────────────────────────

        private void SpawnGhost()
        {
            if (TrapGhostPrefab == null) return;

            _isPlacing = true;
            _yawOffset = 0f;
            _ghostInstance = Instantiate(TrapGhostPrefab);
            _ghostInstance.name = "BearTrapGhost";

            foreach (var col in _ghostInstance.GetComponentsInChildren<Collider>())
                col.enabled = false;

            _ghostRenderers = _ghostInstance.GetComponentsInChildren<Renderer>();

            UpdateGhost();
        }

        private void DestroyGhost()
        {
            _isPlacing = false;

            if (_ghostInstance != null)
            {
                Destroy(_ghostInstance);
                _ghostInstance = null;
                _ghostRenderers = null;
            }
        }

        private void UpdateGhost()
        {
            if (_ghostInstance == null) return;

            bool resolved = TryGetPlacement(out Vector3 point, out Quaternion rotation, out bool isValid);
            _lastPlacementValid = resolved && isValid;

            if (resolved)
            {
                _ghostInstance.SetActive(true);
                _ghostInstance.transform.SetPositionAndRotation(point, rotation);
            }
            else
            {
                _ghostInstance.SetActive(false);
            }

            if (_ghostRenderers == null) return;
            Color tint = _lastPlacementValid ? ValidColor : InvalidColor;
            foreach (var r in _ghostRenderers)
            {
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor(ColorPropID, tint);
                r.SetPropertyBlock(_mpb);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Placement
        // ─────────────────────────────────────────────────────────────

        private void PlaceTrap(Vector3 point, Quaternion rotation)
        {
            if (TrapPrefab == null) return;

            GameObject trap = Instantiate(TrapPrefab, point, rotation);
            trap.name = "BearTrap";
            _activeTraps++;

            if (trap.GetComponent<BearTrapObject>() != null)
                StartCoroutine(TrackTrapLifetime(trap));
        }

        private IEnumerator TrackTrapLifetime(GameObject trap)
        {
            yield return new WaitUntil(() => trap == null);
            _activeTraps = Mathf.Max(0, _activeTraps - 1);
        }

        // ─────────────────────────────────────────────────────────────
        // Aim & Placement Calculation
        // ─────────────────────────────────────────────────────────────

        private bool TryGetPlacement(out Vector3 worldPoint, out Quaternion rotation, out bool isValid)
        {
            worldPoint = Vector3.zero;
            rotation = Quaternion.identity;
            isValid = false;

            if (_cam == null) return false;

            Transform origin = Manager.AbilityOrigin != null ? Manager.AbilityOrigin : transform;

            // ── Step 1: aim point from screen-centre ray ─────────────
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 aimPoint;
            Vector3 surfaceNormal = Vector3.up;

            if (Physics.Raycast(ray, out RaycastHit hit, MaxDistance * 2f, GroundLayer))
            {
                aimPoint = hit.point;
                surfaceNormal = hit.normal;
            }
            else
            {
                Plane groundPlane = new Plane(Vector3.up, origin.position);
                if (!groundPlane.Raycast(ray, out float t)) return false;
                aimPoint = ray.GetPoint(t);
            }

            // ── Step 2: clamp horizontal distance ───────────────────
            Vector3 toAim = aimPoint - origin.position;
            toAim.y = 0f;

            if (toAim.sqrMagnitude < 0.001f) toAim = origin.forward;
            Vector3 flatDir = toAim.normalized;
            float distance = Mathf.Clamp(toAim.magnitude, MinDistance, MaxDistance);

            worldPoint = origin.position + flatDir * distance;

            // ── Step 3: ground-snap ──────────────────────────────────
            // Fire a fresh downward ray at the clamped XZ to get the true
            // ground Y there. Also refreshes surfaceNormal for slope alignment.
            // Fixes the Y/XZ mismatch that caused floating.
            Vector3 sampleOrigin = new Vector3(worldPoint.x, origin.position.y + SnapRayOriginHeight, worldPoint.z);
            if (Physics.Raycast(sampleOrigin, Vector3.down, out RaycastHit snapHit, SnapRayDistance, GroundLayer))
            {
                worldPoint.y = snapHit.point.y + GroundOffset;
                surfaceNormal = snapHit.normal; // use accurate normal for slope alignment below
            }
            else
            {
                worldPoint.y = aimPoint.y + GroundOffset; // fallback if snap ray misses (e.g. over a void)
            }

            // ── Step 4: rotation — trap faces player + scroll yaw ───
            float baseYaw = Mathf.Atan2(-flatDir.x, -flatDir.z) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0f, baseYaw + _yawOffset, 0f);

            // ── Step 5: align to surface normal (sloped ground) ─────
            if (surfaceNormal != Vector3.up)
            {
                Quaternion surfaceAlign = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
                rotation = surfaceAlign * rotation;
            }

            // ── Step 6: obstruction check ────────────────────────────
            isValid = ObstacleLayer == 0
                || !Physics.CheckBox(worldPoint, OverlapHalfExtents, rotation, ObstacleLayer);

            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Input
        // ─────────────────────────────────────────────────────────────

        private void ReadScrollInput()
        {
            if (_scrollAction == null) return;
            float scroll = _scrollAction.ReadValue<float>();
            if (Mathf.Abs(scroll) <= 0.01f) return;
            _yawOffset += scroll > 0f ? RotationStep : -RotationStep;
            _yawOffset = Mathf.Repeat(_yawOffset, 360f);
        }
    }
}