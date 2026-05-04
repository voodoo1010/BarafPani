using _Features.Abilities.Core.Scripts;
using CustomInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Abilities.Hunter
{
    public class IceWall : AbilityBase
    {
        // ─────────────────────────────────────────────────────────────
        // Inspector
        // ─────────────────────────────────────────────────────────────

        [Header("Segment Settings")]
        [Tooltip("Wall segment prefab (real, with collider/HP)"), ForceFill]
        public GameObject wallSegmentPrefab;

        [Tooltip("Transparent ghost preview prefab"), ForceFill]
        public GameObject wallGhostSegmentPrefab;

        [Range(1, 8)] public int segmentCount = 4;
        [Range(0f, 2f)] public float segmentWidth = 1f;
        [Range(0f, 1f)] public float segmentGap = 0.05f;

        [Header("Placement Range")]
        [Range(1f, 5f), Tooltip("Minimum placement distance from origin")]
        public float minDistance = 2f;

        [Range(5f, 30f), Tooltip("Maximum placement distance from origin")]
        public float maxDistance = 12f;

        [Range(0f, 3f), Tooltip("Extra height added above the snapped ground point")]
        public float heightOffset = 0.05f;

        [Header("Ground Snap")]
        [Tooltip("How far above the clamped XZ point the snap ray starts")]
        public float snapRayOriginHeight = 10f;

        [Tooltip("Maximum downward distance the snap ray travels")]
        public float snapRayDistance = 20f;

        [Header("Rotation")]
        [Range(5f, 45f), Tooltip("Degrees rotated per scroll tick")]
        public float rotationStep = 15f;

        [Header("Placement Validation")]
        [Tooltip("Layers treated as valid ground")]
        public LayerMask groundLayer = ~0;

        [Tooltip("Layers that block placement. Set to Nothing to skip check.")]
        public LayerMask obstacleLayer;

        [Tooltip("Half-extents of the overlap box used to check each segment for obstructions")]
        public Vector3 segmentOverlapHalfExtents = new(0.4f, 0.8f, 0.4f);

        [Header("Ghost Colors")]
        public Color validColor = new(0.2f, 0.85f, 1f, 0.45f);
        public Color invalidColor = new(1f, 0.2f, 0.2f, 0.45f);

        [Header("UI (Optional)")]
        public UnityEngine.UI.Image cooldownRadialUI;

        // ─────────────────────────────────────────────────────────────
        // Private state
        // ─────────────────────────────────────────────────────────────

        private bool _isPreviewing;
        private float _yawOffset;
        private GameObject[] _ghostSegments;
        private Renderer[][] _ghostRenderers;
        private bool _lastPlacementValid;

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

            _scrollAction = new InputAction("IceWallScroll", binding: "<Mouse>/scroll/y");
            _scrollAction.Enable();

            if (wallSegmentPrefab == null)
                Debug.LogError("[IceWall] wallSegmentPrefab not assigned!");
            if (wallGhostSegmentPrefab == null)
                Debug.LogError("[IceWall] wallGhostSegmentPrefab not assigned!");
        }

        protected override void OnReleasedInternal()
        {
            DestroyGhosts();
            _scrollAction?.Dispose();
            _scrollAction = null;
        }

        // ─────────────────────────────────────────────────────────────
        // Phase 1 — Activate
        // ─────────────────────────────────────────────────────────────

        protected override bool OnActivateInternal()
        {
            if (_isPreviewing) return false;
            StartPreview();
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Phase 2 — Ability Attack
        // ─────────────────────────────────────────────────────────────

        public override bool OnAbilityAttackInternal()
        {
            if (!_isPreviewing) return false;
            if (!TryGetPlacement(out Vector3 center, out Quaternion rotation, out bool isValid)) return false;

            if (!isValid)
            {
                Debug.Log("[IceWall] Placement blocked — obstruction or out of range.");
                return false;
            }

            SpawnWall(center, rotation);
            DestroyGhosts();
            ConsumeActivation();
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // Cancel
        // ─────────────────────────────────────────────────────────────

        public override void OnCancelInternal()
        {
            DestroyGhosts();
        }

        // ─────────────────────────────────────────────────────────────
        // Update
        // ─────────────────────────────────────────────────────────────

        protected override void Update()
        {
            base.Update();

            if (cooldownRadialUI != null)
                cooldownRadialUI.fillAmount = CooldownNormalized;

            if (!_isPreviewing) return;

            ReadScrollInput();
            UpdateGhostPositions();
        }

        // ─────────────────────────────────────────────────────────────
        // Ghost management
        // ─────────────────────────────────────────────────────────────

        private void StartPreview()
        {
            if (wallGhostSegmentPrefab == null) return;

            _isPreviewing = true;
            _yawOffset = 0f;
            _ghostSegments = new GameObject[segmentCount];
            _ghostRenderers = new Renderer[segmentCount][];

            for (int i = 0; i < segmentCount; i++)
            {
                _ghostSegments[i] = Instantiate(wallGhostSegmentPrefab);
                _ghostSegments[i].name = $"IceWallGhost_{i}";
                _ghostRenderers[i] = _ghostSegments[i].GetComponentsInChildren<Renderer>();

                foreach (var col in _ghostSegments[i].GetComponentsInChildren<Collider>())
                    col.enabled = false;
            }

            UpdateGhostPositions();
        }

        private void DestroyGhosts()
        {
            _isPreviewing = false;

            if (_ghostSegments == null) return;
            foreach (var g in _ghostSegments)
                if (g != null) Destroy(g);

            _ghostSegments = null;
            _ghostRenderers = null;
        }

        private void UpdateGhostPositions()
        {
            if (_ghostSegments == null) return;

            bool resolved = TryGetPlacement(out Vector3 center, out Quaternion rotation, out bool isValid);
            _lastPlacementValid = resolved && isValid;

            Color tint = _lastPlacementValid ? validColor : invalidColor;
            Vector3[] positions = resolved ? CalculateSegmentPositions(center, rotation) : null;

            for (int i = 0; i < segmentCount; i++)
            {
                if (_ghostSegments[i] == null) continue;

                if (positions != null)
                {
                    _ghostSegments[i].SetActive(true);
                    _ghostSegments[i].transform.SetPositionAndRotation(positions[i], rotation);
                }
                else
                {
                    _ghostSegments[i].SetActive(false);
                }

                if (_ghostRenderers[i] == null) continue;
                foreach (var r in _ghostRenderers[i])
                {
                    r.GetPropertyBlock(_mpb);
                    _mpb.SetColor(ColorPropID, tint);
                    r.SetPropertyBlock(_mpb);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Spawning
        // ─────────────────────────────────────────────────────────────

        private void SpawnWall(Vector3 center, Quaternion rotation)
        {
            if (wallSegmentPrefab == null) return;

            Vector3[] positions = CalculateSegmentPositions(center, rotation);
            for (int i = 0; i < segmentCount; i++)
            {
                GameObject seg = Instantiate(wallSegmentPrefab, positions[i], rotation);
                seg.name = $"IceWallSegment_{i}";
            }
        }

        // ─────────────────────────────────────────────────────────────
        // Aim & Placement Calculation
        // ─────────────────────────────────────────────────────────────

        private bool TryGetPlacement(out Vector3 center, out Quaternion rotation, out bool isValid)
        {
            center = Vector3.zero;
            rotation = Quaternion.identity;
            isValid = false;

            if (_cam == null) return false;

            Transform origin = Manager.AbilityOrigin != null ? Manager.AbilityOrigin : transform;

            // ── Step 1: aim point from screen-centre ray ─────────────
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 aimPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance * 2f, groundLayer))
                aimPoint = hit.point;
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
            float distance = Mathf.Clamp(toAim.magnitude, minDistance, maxDistance);

            center = origin.position + flatDir * distance;

            // ── Step 3: ground-snap ──────────────────────────────────
            // Fire a fresh downward ray at the clamped XZ to get the true
            // ground Y there. Fixes the Y/XZ mismatch that caused floating.
            Vector3 sampleOrigin = new Vector3(center.x, origin.position.y + snapRayOriginHeight, center.z);
            if (Physics.Raycast(sampleOrigin, Vector3.down, out RaycastHit snapHit, snapRayDistance, groundLayer))
                center.y = snapHit.point.y + heightOffset;
            else
                center.y = aimPoint.y + heightOffset; // fallback if snap ray misses (e.g. over a void)

            // ── Step 4: rotation (wall perpendicular to aim + scroll) ─
            float baseYaw = Mathf.Atan2(flatDir.x, flatDir.z) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0f, baseYaw + 90f + _yawOffset, 0f);

            // ── Step 5: obstruction check ────────────────────────────
            isValid = obstacleLayer == 0 || IsPlacementClear(center, rotation);
            return true;
        }

        private bool IsPlacementClear(Vector3 center, Quaternion rotation)
        {
            foreach (var pos in CalculateSegmentPositions(center, rotation))
                if (Physics.CheckBox(pos, segmentOverlapHalfExtents, rotation, obstacleLayer))
                    return false;
            return true;
        }

        private Vector3[] CalculateSegmentPositions(Vector3 center, Quaternion rotation)
        {
            var positions = new Vector3[segmentCount];
            float totalWidth = segmentCount * segmentWidth + (segmentCount - 1) * segmentGap;
            float startOffset = -totalWidth * 0.5f + segmentWidth * 0.5f;
            Vector3 right = rotation * Vector3.right;

            for (int i = 0; i < segmentCount; i++)
                positions[i] = center + right * (startOffset + i * (segmentWidth + segmentGap));

            return positions;
        }

        // ─────────────────────────────────────────────────────────────
        // Input
        // ─────────────────────────────────────────────────────────────

        private void ReadScrollInput()
        {
            if (_scrollAction == null) return;
            float scroll = _scrollAction.ReadValue<float>();
            if (Mathf.Abs(scroll) <= 0.01f) return;
            _yawOffset += scroll > 0f ? rotationStep : -rotationStep;
            _yawOffset = Mathf.Repeat(_yawOffset, 360f);
        }
    }
}