using UnityEngine;
using UnityEngine.InputSystem;

public class HookController : MonoBehaviour
{
    [Header("Hook Settings")]
    [Tooltip("The speed at which the hook moves")]
    [SerializeField] private float _hookSpeed = 20f;

    [Tooltip("The speed at which the target moves after being pulled")]
    [SerializeField] private float _pullSpeed = 15f;

    [Tooltip("The maximum distance at which the hook can be pulled")]
    [SerializeField] private float _maxHookDistance = 30f;
    [SerializeField] private Transform _hookOrigin;
    [SerializeField] private Camera _fpsCamera;

    [Header("Layer")]
    [SerializeField] private LayerMask _runnerLayer;

    [Header("Visuals")]
    [SerializeField] private Material _hookLineMaterial;

    private TestingControls _controls;
    private HookProjectile _activeHook;

    private void Awake()
    {
        _controls = new TestingControls();
    }

    private void OnEnable()
    {
        _controls.Test.Enable();
        _controls.Test.fireHook.performed += OnFireHookPerformed;
    }

    private void OnDisable()
    {
        _controls.Test.fireHook.performed -= OnFireHookPerformed;
        _controls.Test.Disable();
    }

    private void OnFireHookPerformed(InputAction.CallbackContext context)
    {
        if (_activeHook != null) return;
        FireHook();
    }

    private void FireHook()
    {
        // Raycast from center of screen
        Ray aimRay = _fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint;
        if (Physics.Raycast(aimRay, out RaycastHit hit, _maxHookDistance))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = aimRay.origin + aimRay.direction * _maxHookDistance;
        }

        // Aim direction
        Vector3 aimDirection = (targetPoint - _hookOrigin.position).normalized;

        // Spawn hook object
        var hookGo = new GameObject("Hook");
        _activeHook = hookGo.AddComponent<HookProjectile>();

        _activeHook.Initialize(
            origin: _hookOrigin,
            direction: aimDirection,
            hookSpeed: _hookSpeed,
            pullSpeed: _pullSpeed,
            maxDistance: _maxHookDistance,
            runnerLayer: _runnerLayer,
            lineMaterial: _hookLineMaterial,
            onFinished: () => _activeHook = null
        );
    }
}
