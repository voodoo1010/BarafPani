using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BearTrapAbility : MonoBehaviour
{
    // -----------------------------------------------------------------------
    //  Inspector
    // -----------------------------------------------------------------------

    [Header("Prefabs")]
    [Tooltip("The actual bear trap prefab. Must have BearTrapObject.cs + a Trigger Collider.")]
    public GameObject TrapPrefab;

    [Tooltip("Ghost/preview prefab shown during placement (transparent, no BearTrapObject script).")]
    public GameObject TrapGhostPrefab;

    [Header("Placement")]
    [Tooltip("Maximum distance from the player a trap can be placed.")]
    public float MaxPlaceDistance = 8f;

    [Tooltip("Y offset so the trap sits flush on the ground surface.")]
    public float GroundOffset = 0.05f;

    [Header("Trap Limit")]
    [Tooltip("Maximum number of traps that can exist at once. 0 = unlimited.")]
    public int MaxTrapsActive = 3;

    [Header("Cooldown")]
    [Tooltip("Cooldown in seconds between each trap placement.")]
    public float PlaceCooldown = 8f;

    [Header("Layer Mask")]
    [Tooltip("Layers counted as valid ground for trap placement.")]
    public LayerMask GroundLayer;

    [Header("UI (Optional)")]
    [Tooltip("Radial UI Image for cooldown display (fill type: Radial 360).")]
    public UnityEngine.UI.Image CooldownRadialUI;

    // -----------------------------------------------------------------------
    //  Private State
    // -----------------------------------------------------------------------

    private bool _isPlacing;
    private GameObject _ghostInstance;
    private Camera _camera;
    private float _cooldownTimer;
    private bool _onCooldown;
    private int _activeTraps;

    private static readonly Color ColourValid = new(0.7f, 0.45f, 0.1f, 0.55f);
    private static readonly Color ColourInvalid = new(1f, 0.15f, 0.15f, 0.55f);

    // -----------------------------------------------------------------------
    //  Input Actions
    // -----------------------------------------------------------------------

    private InputAction _activateAction;
    private InputAction _placeAction;
    private InputAction _cancelAction;

    // -----------------------------------------------------------------------
    //  Unity Lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        _activateAction = new InputAction("TrapActivate", binding: "<Keyboard>/b");
        _activateAction.performed += _ => HandleActivation();

        _placeAction = new InputAction("TrapPlace", binding: "<Mouse>/leftButton");
        _placeAction.performed += _ => HandlePlace();

        _cancelAction = new InputAction("TrapCancel", binding: "<Mouse>/rightButton");
        _cancelAction.performed += _ =>
        {
            if (_isPlacing)
                CancelPlacement();
        };

        _activateAction.Enable();
        _placeAction.Enable();
        _cancelAction.Enable();
    }

    private void Start()
    {
        _camera = Camera.main;

        if (_camera == null)
            Debug.LogError("[BearTrap] No MainCamera found! Tag your camera as MainCamera.");

        if (TrapPrefab == null)
            Debug.LogError("[BearTrap] TrapPrefab is not assigned!");

        if (TrapGhostPrefab == null)
            Debug.LogError("[BearTrap] TrapGhostPrefab is not assigned!");
    }

    private void Update()
    {
        TickCooldown();

        if (_isPlacing)
            UpdateGhost();
    }

    private void OnDestroy()
    {
        _activateAction?.Dispose();
        _placeAction?.Dispose();
        _cancelAction?.Dispose();
    }

    // -----------------------------------------------------------------------
    //  Cooldown
    // -----------------------------------------------------------------------

    private void TickCooldown()
    {
        if (!_onCooldown)
            return;

        _cooldownTimer -= Time.deltaTime;

        if (CooldownRadialUI != null)
            CooldownRadialUI.fillAmount = _cooldownTimer / PlaceCooldown;

        if (_cooldownTimer <= 0f)
        {
            _onCooldown = false;

            if (CooldownRadialUI != null)
                CooldownRadialUI.fillAmount = 0f;

            Debug.Log("[BearTrap] Ready to place!");
        }
    }

    // -----------------------------------------------------------------------
    //  Activation
    // -----------------------------------------------------------------------

    private void HandleActivation()
    {
        if (_onCooldown)
        {
            Debug.Log($"[BearTrap] On cooldown — {_cooldownTimer:F1}s remaining.");
            return;
        }

        if (MaxTrapsActive > 0 && _activeTraps >= MaxTrapsActive)
        {
            Debug.Log($"[BearTrap] Trap limit reached ({MaxTrapsActive}). Wait for one to expire.");
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

        if (TrapGhostPrefab == null)
            return;

        _ghostInstance = Instantiate(TrapGhostPrefab);
        _ghostInstance.name = "BearTrapGhost";

        DisableAllColliders(_ghostInstance);

        Debug.Log("[BearTrap] Placement mode ON — LMB to place | RMB to cancel");
    }

    private void CancelPlacement()
    {
        _isPlacing = false;
        DestroyGhost();
        Debug.Log("[BearTrap] Placement cancelled.");
    }

    // -----------------------------------------------------------------------
    //  Ghost Visualizer
    // -----------------------------------------------------------------------

    private void UpdateGhost()
    {
        if (_ghostInstance == null)
            return;

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

    // -----------------------------------------------------------------------
    //  Placement
    // -----------------------------------------------------------------------

    private void HandlePlace()
    {
        if (!_isPlacing)
            return;

        bool hit = GetGroundPoint(out Vector3 point, out bool isValid);

        if (hit && isValid)
            SpawnTrap(point);
        else
            Debug.Log("[BearTrap] Can't place here — out of range or no valid surface.");
    }

    private void SpawnTrap(Vector3 position)
    {
        if (TrapPrefab == null)
            return;

        GameObject trap = Instantiate(TrapPrefab, position, Quaternion.identity);
        trap.name = "BearTrap";

        _activeTraps++;

        if (trap.GetComponent<BearTrapObject>() != null)
            StartCoroutine(TrackTrapLifetime(trap));

        _isPlacing = false;
        _onCooldown = true;
        _cooldownTimer = PlaceCooldown;

        DestroyGhost();

        Debug.Log($"[BearTrap] Trap placed at {position}.");
    }

    private IEnumerator TrackTrapLifetime(GameObject trap)
    {
        while (trap != null)
            yield return null;

        _activeTraps = Mathf.Max(0, _activeTraps - 1);

        Debug.Log($"[BearTrap] Trap destroyed. Active traps: {_activeTraps}/{MaxTrapsActive}");
    }

    // -----------------------------------------------------------------------
    //  Raycasting
    // -----------------------------------------------------------------------

    private bool GetGroundPoint(out Vector3 worldPoint, out bool isValid)
    {
        worldPoint = Vector3.zero;
        isValid = false;

        if (_camera == null || Mouse.current == null)
            return false;

        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GroundLayer))
            return false;

        worldPoint = hit.point + Vector3.up * GroundOffset;
        isValid = Vector3.Distance(transform.position, hit.point) <= MaxPlaceDistance;

        return true;
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private void DestroyGhost()
    {
        if (_ghostInstance == null)
            return;

        Destroy(_ghostInstance);
        _ghostInstance = null;
    }

    private void SetGhostColour(Color colour)
    {
        if (_ghostInstance == null)
            return;

        foreach (Renderer r in _ghostInstance.GetComponentsInChildren<Renderer>())
        {
            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                    mat.color = colour;

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", colour);
            }
        }
    }

    private void DisableAllColliders(GameObject go)
    {
        foreach (Collider col in go.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    // -----------------------------------------------------------------------
    //  Gizmos
    // -----------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, MaxPlaceDistance);
    }
}