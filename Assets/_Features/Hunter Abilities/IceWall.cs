using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Ice Wall Ability (Multi-Segment Version)
/// Attach this to your Player GameObject.
/// </summary>
public class IceWall : MonoBehaviour
{
    // -----------------------------------------------------------------------
    // Inspector — Wall Composition
    // -----------------------------------------------------------------------

    [Header("Segment Settings")]
    [Tooltip("The individual cube segment prefab. Must have IceWallSegment.cs on it.")]
    public GameObject wallSegmentPrefab;

    [Tooltip("Ghost/preview segment prefab (transparent blue material, NO IceWallSegment script needed).")]
    public GameObject wallGhostSegmentPrefab;

    [Tooltip("How many segments the wall is built from. (Recommended: 3 or 4)")]
    [Range(1, 8)]
    public int segmentCount = 4;

    [Tooltip("Gap between each segment (0 = flush). Small values like 0.05 look natural.")]
    public float segmentGap = 0.05f;

    // -----------------------------------------------------------------------
    // Inspector — Placement
    // -----------------------------------------------------------------------

    [Header("Placement")]
    [Tooltip("Maximum distance from player the wall can be placed.")]
    public float maxPlaceDistance = 10f;

    [Tooltip("Height offset so segments sit correctly on the ground.")]
    public float wallHeightOffset = 1f;

    // -----------------------------------------------------------------------
    // Inspector — Rotation
    // -----------------------------------------------------------------------

    [Header("Rotation")]
    [Tooltip("Degrees rotated per scroll tick.")]
    public float rotationStep = 15f;

    // -----------------------------------------------------------------------
    // Inspector — Cooldown
    // -----------------------------------------------------------------------

    [Header("Cooldown")]
    public float abilityCooldown = 30f;

    // -----------------------------------------------------------------------
    // Inspector — Layer Mask
    // -----------------------------------------------------------------------

    [Header("Layer Mask")]
    [Tooltip("Layer(s) that count as valid ground for wall placement.")]
    public LayerMask groundLayer;

    // -----------------------------------------------------------------------
    // Inspector — Optional UI
    // -----------------------------------------------------------------------

    [Header("UI (Optional)")]
    [Tooltip("Radial Image shown during cooldown (fill type: Radial 360).")]
    public UnityEngine.UI.Image cooldownRadialUI;

    // -----------------------------------------------------------------------
    // Private State
    // -----------------------------------------------------------------------

    private bool _isPlacing = false;
    private float _currentRotation = 0f;
    private Camera _cam;
    private float _cooldownTimer = 0f;
    private bool _onCooldown = false;

    private GameObject[] _ghostSegments;

    private static readonly Color ColourValid = new Color(0.2f, 0.85f, 1f, 0.45f);
    private static readonly Color ColourInvalid = new Color(1f, 0.2f, 0.2f, 0.45f);

    // -----------------------------------------------------------------------
    // Input Actions
    // -----------------------------------------------------------------------

    private InputAction _activateAction;
    private InputAction _placeAction;
    private InputAction _cancelAction;
    private InputAction _scrollAction;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        _activateAction = new InputAction("Activate", binding: "<Keyboard>/e");
        _activateAction.performed += _ => HandleActivation();

        _placeAction = new InputAction("Place", binding: "<Mouse>/leftButton");
        _placeAction.performed += _ => HandlePlace();

        _cancelAction = new InputAction("Cancel", binding: "<Mouse>/rightButton");
        _cancelAction.performed += _ =>
        {
            if (_isPlacing)
                CancelPlacement();
        };

        _scrollAction = new InputAction("Scroll", binding: "<Mouse>/scroll/y");

        _activateAction.Enable();
        _placeAction.Enable();
        _cancelAction.Enable();
        _scrollAction.Enable();
    }

    private void Start()
    {
        _cam = Camera.main;

        if (_cam == null)
            Debug.LogError("[IceWall] No MainCamera found! Tag your camera as MainCamera.");

        if (wallSegmentPrefab == null)
            Debug.LogError("[IceWall] wallSegmentPrefab is not assigned!");

        if (wallGhostSegmentPrefab == null)
            Debug.LogError("[IceWall] wallGhostSegmentPrefab is not assigned!");
    }

    private void Update()
    {
        TickCooldown();

        if (_isPlacing)
        {
            HandleRotationInput();
            UpdateGhostVisualizer();
        }
    }

    private void OnDestroy()
    {
        _activateAction?.Dispose();
        _placeAction?.Dispose();
        _cancelAction?.Dispose();
        _scrollAction?.Dispose();
    }

    // -----------------------------------------------------------------------
    // Cooldown
    // -----------------------------------------------------------------------

    private void TickCooldown()
    {
        if (!_onCooldown)
            return;

        _cooldownTimer -= Time.deltaTime;

        if (cooldownRadialUI != null)
            cooldownRadialUI.fillAmount = _cooldownTimer / abilityCooldown;

        if (_cooldownTimer <= 0f)
        {
            _onCooldown = false;

            if (cooldownRadialUI != null)
                cooldownRadialUI.fillAmount = 0f;

            Debug.Log("[IceWall] Ability ready!");
        }
    }

    // -----------------------------------------------------------------------
    // Activation
    // -----------------------------------------------------------------------

    private void HandleActivation()
    {
        if (_onCooldown)
        {
            Debug.Log($"[IceWall] On cooldown — {_cooldownTimer:F1}s remaining.");
            return;
        }

        if (_isPlacing)
            CancelPlacement();
        else
            EnterPlacementMode();
    }

    private void EnterPlacementMode()
    {
        _isPlacing = true;
        _currentRotation = 0f;

        _ghostSegments = new GameObject[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            if (wallGhostSegmentPrefab == null)
                continue;

            _ghostSegments[i] = Instantiate(wallGhostSegmentPrefab);
            _ghostSegments[i].name = $"IceWallGhost_{i}";
        }

        Debug.Log("[IceWall] Placement mode ON — Scroll to rotate | LMB to place | RMB to cancel");
    }

    private void CancelPlacement()
    {
        _isPlacing = false;
        DestroyAllGhosts();
        Debug.Log("[IceWall] Placement cancelled.");
    }

    // -----------------------------------------------------------------------
    // Rotation Input
    // -----------------------------------------------------------------------

    private void HandleRotationInput()
    {
        float scroll = _scrollAction.ReadValue<float>();

        if (Mathf.Abs(scroll) <= 0.01f)
            return;

        _currentRotation += scroll > 0 ? rotationStep : -rotationStep;
        _currentRotation = Mathf.Repeat(_currentRotation, 360f);
    }

    // -----------------------------------------------------------------------
    // Ghost Visualizer
    // -----------------------------------------------------------------------

    private void UpdateGhostVisualizer()
    {
        if (_ghostSegments == null)
            return;

        bool hitFound = GetPlacementData(out Vector3 centre, out bool isValid);

        if (!hitFound)
        {
            foreach (GameObject g in _ghostSegments)
                if (g != null)
                    g.SetActive(false);

            return;
        }

        Vector3[] positions = CalculateSegmentPositions(centre);
        Quaternion rotation = Quaternion.Euler(0f, _currentRotation, 0f);
        Color colour = isValid ? ColourValid : ColourInvalid;

        for (int i = 0; i < segmentCount; i++)
        {
            if (_ghostSegments[i] == null)
                continue;

            _ghostSegments[i].SetActive(true);
            _ghostSegments[i].transform.position = positions[i];
            _ghostSegments[i].transform.rotation = rotation;

            SetGhostColour(_ghostSegments[i], colour);
        }
    }

    // -----------------------------------------------------------------------
    // Placement
    // -----------------------------------------------------------------------

    private void HandlePlace()
    {
        if (!_isPlacing)
            return;

        bool hitFound = GetPlacementData(out Vector3 centre, out bool isValid);

        if (hitFound && isValid)
            PlaceWall(centre);
        else
            Debug.Log("[IceWall] Can't place — out of range or invalid surface.");
    }

    private void PlaceWall(Vector3 centre)
    {
        if (wallSegmentPrefab == null)
            return;

        Vector3[] positions = CalculateSegmentPositions(centre);
        Quaternion rotation = Quaternion.Euler(0f, _currentRotation, 0f);

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject seg = Instantiate(wallSegmentPrefab, positions[i], rotation);
            seg.name = $"IceWallSegment_{i}";
        }

        _isPlacing = false;
        _onCooldown = true;
        _cooldownTimer = abilityCooldown;

        DestroyAllGhosts();

        Debug.Log($"[IceWall] Wall placed ({segmentCount} segments) at {centre}. Cooldown started.");
    }

    // -----------------------------------------------------------------------
    // Maths
    // -----------------------------------------------------------------------

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
        if (wallSegmentPrefab == null)
            return 1f;

        Renderer rend = wallSegmentPrefab.GetComponentInChildren<Renderer>();

        if (rend != null && rend.bounds.size.x > 0.01f)
            return rend.bounds.size.x;

        return wallSegmentPrefab.transform.localScale.x;
    }

    // -----------------------------------------------------------------------
    // Raycasting
    // -----------------------------------------------------------------------

    private bool GetPlacementData(out Vector3 worldPoint, out bool isValid)
    {
        worldPoint = Vector3.zero;
        isValid = false;

        if (_cam == null || Mouse.current == null)
            return false;

        Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, maxPlaceDistance * 2f, groundLayer))
            return false;

        worldPoint = hit.point + Vector3.up * wallHeightOffset;
        isValid = Vector3.Distance(transform.position, hit.point) <= maxPlaceDistance;

        return true;
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void DestroyAllGhosts()
    {
        if (_ghostSegments == null)
            return;

        foreach (GameObject g in _ghostSegments)
            if (g != null)
                Destroy(g);

        _ghostSegments = null;
    }

    private void SetGhostColour(GameObject ghost, Color c)
    {
        if (ghost == null)
            return;

        foreach (Renderer r in ghost.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                    mat.color = c;

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", c);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, maxPlaceDistance);
    }
}