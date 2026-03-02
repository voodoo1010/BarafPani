using System.Collections;
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

    [Tooltip("The cooldown between shots")]
    [SerializeField] private float _fireCooldown = 1.5f;

    [SerializeField] private Transform _hookOrigin;
    [SerializeField] private Camera _fpsCamera;

    [Header("Layer")]
    [SerializeField] private LayerMask _runnerLayer;

    [Header("Prefabs")]
    [Tooltip("Assign a HookProjectile prefab (must have the HookProjectile component and a configured LineRenderer).")]
    [SerializeField] private HookProjectile _hookPrefab;

    private TestingControls _controls;
    private HookProjectile _activeHook;
    private bool _isOnCooldown;
    private float _cooldownRemaining;

    public bool IsOnCooldown => _isOnCooldown;
    public float CooldownRemaining => (_isOnCooldown ? _cooldownRemaining : 0f);

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
        if (_activeHook != null || _isOnCooldown) return;
        FireHook();
    }

    private void FireHook()
    {
        if (_hookPrefab == null)
        {
            Debug.LogError("HookController: _hookPrefab is not assigned in inspector.");
            return;
        }

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

        HookProjectile hookInstance = Instantiate(_hookPrefab, _hookOrigin.position, Quaternion.identity);
        _activeHook = hookInstance;

        _activeHook.Initialize(
            origin: _hookOrigin,
            direction: aimDirection,
            hookSpeed: _hookSpeed,
            pullSpeed: _pullSpeed,
            maxDistance: _maxHookDistance,
            runnerLayer: _runnerLayer,
            onFinished: OnHookFinished
        );
    }

    private void OnHookFinished()
    {
        _activeHook = null;
        StartCoroutine(FireCooldownRoutine());
    }

    private IEnumerator FireCooldownRoutine()
    {
        _isOnCooldown = true;
        _cooldownRemaining = _fireCooldown;

        while (_cooldownRemaining > 0f)
        {
            _cooldownRemaining -= Time.deltaTime;
            yield return null;
        }

        _isOnCooldown = false;
        _cooldownRemaining = 0f;
    }
}