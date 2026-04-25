using _Features.Abilities.Core.Scripts;
using CustomInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Abilities.Hunter
{
    public class IceWall : AbilityBase
    {
        [Header("Segment Settings")]
        [Tooltip("Wall segment prefab (real, with collider/HP)"), ForceFill]
        public GameObject wallSegmentPrefab;
        [Tooltip("Transparent ghost preview prefab"), ForceFill]
        public GameObject wallGhostSegmentPrefab;

        [Range(1, 8)] public int segmentCount = 4;
        [Range(0f, 2f)] public float segmentWidth = 1f;
        [Range(0f, 1f)] public float segmentGap = 0.05f;

        [Header("Aim & Distance")]
        [Range(1f, 5f), Tooltip("Minimum placement distance (when looking down)")]
        public float minDistance = 2f;

        [Range(5f, 30f), Tooltip("Maximum placement distance (when looking forward)")]
        public float maxDistance = 12f;

        [Range(0f, 5f), Tooltip("Vertical offset above ground")]
        public float heightOffset = 1f;

        [Header("Rotation")]
        [Range(5f, 45f), Tooltip("Degrees per scroll tick")]
        public float rotationStep = 15f;

        [Header("Layer Mask")]
        [Tooltip("Layers considered valid ground")]
        public LayerMask groundLayer = ~0;  // Default to "Everything"

        [Header("Validation")]
        [Tooltip("Color when placement is valid")]
        public Color validColor = new(0.2f, 0.85f, 1f, 0.45f);
        [Tooltip("Color when placement is invalid")]
        public Color invalidColor = new(1f, 0.2f, 0.2f, 0.45f);

        [Header("UI (Optional)")]
        public UnityEngine.UI.Image cooldownRadialUI;

        private bool _isPreviewing;
        private float _yawOffset;          // Scroll wheel rotation
        private GameObject[] _ghostSegments;
        private Camera _cam;

        private InputAction _scrollAction;

        protected override void OnAcquiredInternal()
        {
            _cam = Camera.main;
            if (_cam == null)
                Debug.LogError("[IceWall] No Main Camera found! Tag your camera as MainCamera.");

            _scrollAction = new InputAction("WallScroll", binding: "<Mouse>/scroll/y");
            _scrollAction.Enable();

            if (wallSegmentPrefab == null)
                Debug.LogError("[IceWall] wallSegmentPrefab not assigned!");
            if (wallGhostSegmentPrefab == null)
                Debug.LogError("[IceWall] wallGhostSegmentPrefab not assigned!");
        }

        protected override void OnReleasedInternal()
        {
            CancelPreview();
            _scrollAction?.Dispose();
            _scrollAction = null;
        }

        protected override void Update()
        {
            base.Update();

            if (cooldownRadialUI != null)
                cooldownRadialUI.fillAmount = CooldownNormalized;

            if (_isPreviewing)
            {
                HandleScrollRotation();
                UpdateGhostPositions();
            }
        }

        protected override bool OnActivateInternal()
        {
            if (!_isPreviewing)
                StartPreview();
            else
                ConfirmPlacement();

            return true;
        }

        // ───────────────────────────────────────────────────────────
        // Preview Lifecycle
        // ───────────────────────────────────────────────────────────

        private void StartPreview()
        {
            if (wallGhostSegmentPrefab == null) return;

            _isPreviewing = true;
            _yawOffset = 0f;

            _ghostSegments = new GameObject[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                _ghostSegments[i] = Instantiate(wallGhostSegmentPrefab);
                _ghostSegments[i].name = $"IceWallGhost_{i}";
            }

            UpdateGhostPositions();
        }

        private void CancelPreview()
        {
            _isPreviewing = false;
            DestroyGhosts();
        }

        private void ConfirmPlacement()
        {
            if (wallSegmentPrefab == null) return;
            if (!TryGetPlacement(out Vector3 center, out Quaternion rotation, out bool isValid)) return;
            if (!isValid)
            {
                Debug.Log("[IceWall] Invalid placement.");
                return;
            }

            Vector3[] positions = CalculateSegmentPositions(center, rotation);
            for (int i = 0; i < segmentCount; i++)
            {
                GameObject seg = Instantiate(wallSegmentPrefab, positions[i], rotation);
                seg.name = $"IceWallSegment_{i}";
            }

            CancelPreview();
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

            _yawOffset += scroll > 0 ? rotationStep : -rotationStep;
            _yawOffset = Mathf.Repeat(_yawOffset, 360f);
        }

        // ───────────────────────────────────────────────────────────
        // Aim Calculation (Sage-style)
        // ───────────────────────────────────────────────────────────

        private bool TryGetPlacement(out Vector3 center, out Quaternion rotation, out bool isValid)
        {
            center = Vector3.zero;
            rotation = Quaternion.identity;
            isValid = false;

            if (_cam == null) return false;

            // Cast ray FROM camera through screen center (third-person crosshair)
            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            Vector3 aimPoint;

            // Try to hit ground directly with ray
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance * 2f, groundLayer))
            {
                aimPoint = hit.point;
            }
            else
            {
                // No hit — project ray onto a horizontal plane at player's feet level
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
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            Vector3 horizontalDir = toAim.sqrMagnitude > 0.001f ? toAim.normalized : transform.forward;

            // Final placement point
            center = transform.position + horizontalDir * distance + Vector3.up * heightOffset;

            // Wall rotation: face the player, plus scroll wheel offset
            float baseYaw = Mathf.Atan2(horizontalDir.x, horizontalDir.z) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0f, baseYaw + _yawOffset, 0f);

            // Valid if within max range
            isValid = (aimPoint - transform.position).magnitude <= maxDistance * 1.05f;

            return true;
        }

        private Vector3[] CalculateSegmentPositions(Vector3 center, Quaternion rotation)
        {
            Vector3 right = rotation * Vector3.right;
            float totalWidth = (segmentWidth + segmentGap) * segmentCount - segmentGap;
            float startOffset = -totalWidth * 0.5f + segmentWidth * 0.5f;

            Vector3[] positions = new Vector3[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                float offset = startOffset + i * (segmentWidth + segmentGap);
                positions[i] = center + right * offset;
            }
            return positions;
        }

        // ───────────────────────────────────────────────────────────
        // Ghost Update
        // ───────────────────────────────────────────────────────────

        private void UpdateGhostPositions()
        {
            if (_ghostSegments == null) return;

            if (!TryGetPlacement(out Vector3 center, out Quaternion rotation, out bool isValid))
            {
                foreach (GameObject g in _ghostSegments)
                    if (g != null) g.SetActive(false);
                return;
            }

            Vector3[] positions = CalculateSegmentPositions(center, rotation);
            Color color = isValid ? validColor : invalidColor;

            for (int i = 0; i < segmentCount; i++)
            {
                if (_ghostSegments[i] == null) continue;
                _ghostSegments[i].SetActive(true);
                _ghostSegments[i].transform.position = positions[i];
                _ghostSegments[i].transform.rotation = rotation;
                SetGhostColor(_ghostSegments[i], color);
            }
        }

        private void SetGhostColor(GameObject ghost, Color c)
        {
            foreach (Renderer r in ghost.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in r.materials)
                {
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                    else if (mat.HasProperty("_Color")) mat.color = c;
                }
            }
        }

        private void DestroyGhosts()
        {
            if (_ghostSegments == null) return;
            foreach (GameObject g in _ghostSegments)
                if (g != null) Destroy(g);
            _ghostSegments = null;
        }

        private void OnDisable()
        {
            DestroyGhosts();
        }

        // ───────────────────────────────────────────────────────────
        // Editor Gizmos
        // ───────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxDistance);
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, minDistance);
        }
    }
}