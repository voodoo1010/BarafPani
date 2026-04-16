using _Features.Abilities.Core.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Abilities.Hunter
{
    public class IceWall : AbilityBase
    {
        [Header("Segment Settings")]
        public GameObject wallSegmentPrefab;
        public GameObject wallGhostSegmentPrefab;

        [Range(1, 8)] public int segmentCount = 4;
        public float segmentGap = 0.05f;

        [Header("Placement")]
        public float maxPlaceDistance = 10f;
        public float wallHeightOffset = 1f;

        [Header("Rotation")]
        public float rotationStep = 15f;

        [Header("Layer Mask")]
        public LayerMask groundLayer;

        [Header("UI (Optional)")]
        public UnityEngine.UI.Image cooldownRadialUI;

        private bool _isPlacing;
        private float _currentRotation;
        private Camera _cam;
        private GameObject[] _ghostSegments;

        private static readonly Color ColourValid = new(0.2f, 0.85f, 1f, 0.45f);
        private static readonly Color ColourInvalid = new(1f, 0.2f, 0.2f, 0.45f);

        private InputAction _placeAction;
        private InputAction _cancelAction;
        private InputAction _scrollAction;

        protected override void OnAcquiredInternal()
        {
            _cam = Camera.main;

            _placeAction = new InputAction("Place", binding: "<Mouse>/leftButton");
            _placeAction.performed += _ => HandlePlace();

            _cancelAction = new InputAction("Cancel", binding: "<Mouse>/rightButton");
            _cancelAction.performed += _ => { if (_isPlacing) CancelPlacement(); };

            _scrollAction = new InputAction("Scroll", binding: "<Mouse>/scroll/y");

            _placeAction.Enable();
            _cancelAction.Enable();
            _scrollAction.Enable();

            if (wallSegmentPrefab == null) Debug.LogError("[IceWall] wallSegmentPrefab not assigned.");
            if (wallGhostSegmentPrefab == null) Debug.LogError("[IceWall] wallGhostSegmentPrefab not assigned.");
        }

        protected override void OnReleasedInternal()
        {
            if (_isPlacing) CancelPlacement();
            _placeAction?.Dispose();
            _cancelAction?.Dispose();
            _scrollAction?.Dispose();
            _placeAction = null;
            _cancelAction = null;
            _scrollAction = null;
        }

        protected override void Update()
        {
            base.Update();

            if (cooldownRadialUI != null)
                cooldownRadialUI.fillAmount = CooldownNormalized;

            if (_isPlacing)
            {
                HandleRotationInput();
                UpdateGhostVisualizer();
            }
        }

        protected override bool OnActivateInternal()
        {
            if (_isPlacing) CancelPlacement();
            else EnterPlacementMode();
            return true;
        }

        private void EnterPlacementMode()
        {
            _isPlacing = true;
            _currentRotation = 0f;

            _ghostSegments = new GameObject[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                if (wallGhostSegmentPrefab == null) continue;
                _ghostSegments[i] = Instantiate(wallGhostSegmentPrefab);
                _ghostSegments[i].name = $"IceWallGhost_{i}";
            }
        }

        private void CancelPlacement()
        {
            _isPlacing = false;
            DestroyAllGhosts();
        }

        private void HandleRotationInput()
        {
            float scroll = _scrollAction.ReadValue<float>();
            if (Mathf.Abs(scroll) <= 0.01f) return;

            _currentRotation += scroll > 0 ? rotationStep : -rotationStep;
            _currentRotation = Mathf.Repeat(_currentRotation, 360f);
        }

        private void UpdateGhostVisualizer()
        {
            if (_ghostSegments == null) return;

            bool hitFound = GetPlacementData(out Vector3 centre, out bool isValid);

            if (!hitFound)
            {
                foreach (GameObject g in _ghostSegments)
                    if (g != null) g.SetActive(false);
                return;
            }

            Vector3[] positions = CalculateSegmentPositions(centre);
            Quaternion rotation = Quaternion.Euler(0f, _currentRotation, 0f);
            Color colour = isValid ? ColourValid : ColourInvalid;

            for (int i = 0; i < segmentCount; i++)
            {
                if (_ghostSegments[i] == null) continue;
                _ghostSegments[i].SetActive(true);
                _ghostSegments[i].transform.position = positions[i];
                _ghostSegments[i].transform.rotation = rotation;
                SetGhostColour(_ghostSegments[i], colour);
            }
        }

        private void HandlePlace()
        {
            if (!_isPlacing) return;

            bool hitFound = GetPlacementData(out Vector3 centre, out bool isValid);

            if (hitFound && isValid) PlaceWall(centre);
            else Debug.Log("[IceWall] Invalid placement.");
        }

        private void PlaceWall(Vector3 centre)
        {
            if (wallSegmentPrefab == null) return;

            Vector3[] positions = CalculateSegmentPositions(centre);
            Quaternion rotation = Quaternion.Euler(0f, _currentRotation, 0f);

            for (int i = 0; i < segmentCount; i++)
            {
                GameObject seg = Instantiate(wallSegmentPrefab, positions[i], rotation);
                seg.name = $"IceWallSegment_{i}";
            }

            _isPlacing = false;
            DestroyAllGhosts();
            ConsumeActivation();
        }

        private Vector3[] CalculateSegmentPositions(Vector3 centre)
        {
            float segmentWidth = GetSegmentWidth();
            float totalWidth = (segmentWidth + segmentGap) * segmentCount - segmentGap;
            float startX = -totalWidth * 0.5f + segmentWidth * 0.5f;

            Vector3 right = Quaternion.Euler(0f, _currentRotation, 0f) * Vector3.right;
            Vector3[] positions = new Vector3[segmentCount];

            for (int i = 0; i < segmentCount; i++)
            {
                float offset = startX + i * (segmentWidth + segmentGap);
                positions[i] = centre + right * offset;
            }

            return positions;
        }

        private float GetSegmentWidth()
        {
            if (wallSegmentPrefab == null) return 1f;
            Renderer rend = wallSegmentPrefab.GetComponentInChildren<Renderer>();
            if (rend != null && rend.bounds.size.x > 0.01f) return rend.bounds.size.x;
            return wallSegmentPrefab.transform.localScale.x;
        }

        private bool GetPlacementData(out Vector3 worldPoint, out bool isValid)
        {
            worldPoint = Vector3.zero;
            isValid = false;

            if (_cam == null || Mouse.current == null) return false;

            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit, maxPlaceDistance * 2f, groundLayer))
                return false;

            worldPoint = hit.point + Vector3.up * wallHeightOffset;
            isValid = Vector3.Distance(transform.position, hit.point) <= maxPlaceDistance;
            return true;
        }

        private void DestroyAllGhosts()
        {
            if (_ghostSegments == null) return;
            foreach (GameObject g in _ghostSegments)
                if (g != null) Destroy(g);
            _ghostSegments = null;
        }

        private void SetGhostColour(GameObject ghost, Color c)
        {
            if (ghost == null) return;
            foreach (Renderer r in ghost.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in r.materials)
                {
                    if (mat.HasProperty("_Color")) mat.color = c;
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 0.8f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, maxPlaceDistance);
        }
    }
}
