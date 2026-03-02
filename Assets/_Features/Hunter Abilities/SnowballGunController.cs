using UnityEngine;
using UnityEngine.InputSystem;

public class SnowballGunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _muzzlePoint;
    [SerializeField] private SnowballProjectile _snowballPrefab;

    [Header("Fire Settings")]
    [Tooltip("Shots per second")]
    [SerializeField] private float _fireRate = 2f; // rate limiter (time between shots)

    [Tooltip("Speed of the snowball")]
    [SerializeField] private float _snowballSpeed = 20f;

    [Tooltip("Gravity of the snowball")]
    [SerializeField] private float _gravityScale = 9.8f;

    [Header("Ammo Settings")]
    [Tooltip("How many shots the ability allows before going on cooldown")]
    [SerializeField] private int _maxAmmo = 10;

    [Tooltip("Seconds the ability is unavailable after ammo is exhausted")]
    [SerializeField] private float _abilityCooldown = 5f;

    private TestingControls _controls;
    private float _fireCooldownTimer;
    private float _abilityCooldownTimer;
    private int _currentAmmo;
    private bool _isAbilityOnCooldown;
    private Camera _mainCamera;

    private void Awake()
    {
        _controls = new TestingControls();
        _mainCamera = Camera.main;
        _currentAmmo = _maxAmmo;
        _isAbilityOnCooldown = false;
    }

    private void OnEnable()
    {
        _controls.Test.Enable();
        _controls.Test.firegun.performed += OnFiregunPerformed;
    }

    private void OnDisable()
    {
        _controls.Test.firegun.performed -= OnFiregunPerformed;
        _controls.Test.Disable();
    }

    private void Update()
    {
        if (_fireCooldownTimer > 0f)
            _fireCooldownTimer -= Time.deltaTime;

        if (_isAbilityOnCooldown)
        {
            _abilityCooldownTimer -= Time.deltaTime;
            if (_abilityCooldownTimer <= 0f)
                FinishAbilityCooldown();
        }
    }

    private void OnFiregunPerformed(InputAction.CallbackContext context)
    {
        Fire();
    }

    private void Fire()
    {
        if (_fireCooldownTimer > 0f) return;

        if (_isAbilityOnCooldown)
        {
            Debug.Log($"Ability still on cooldown: {_abilityCooldownTimer:0.00}s remaining");
            return;
        }

        if (_currentAmmo <= 0)
        {
            StartAbilityCooldown();
            return;
        }

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, 500f)
            ? hit.point
            : ray.GetPoint(500f);

        Vector3 fireDirection = (targetPoint - _muzzlePoint.position).normalized;

        SnowballProjectile snowball = Instantiate(
            _snowballPrefab,
            _muzzlePoint.position,
            Quaternion.LookRotation(fireDirection)
        );

        snowball.Launch(fireDirection, _snowballSpeed, _gravityScale);

        _currentAmmo--;
        _fireCooldownTimer = 1f / _fireRate;

        Debug.Log($"Fired. Ammo remaining: {_currentAmmo}/{_maxAmmo}");

        if (_currentAmmo <= 0)
            StartAbilityCooldown();
    }

    private void StartAbilityCooldown()
    {
        if (_isAbilityOnCooldown) return;

        _isAbilityOnCooldown = true;
        _abilityCooldownTimer = _abilityCooldown;
        Debug.Log($"Ability used up. Entering cooldown for {_abilityCooldown:0.00}s");
        //  disable HUD ability icon / play VFX
    }

    private void FinishAbilityCooldown()
    {
        _isAbilityOnCooldown = false;
        _currentAmmo = _maxAmmo;
        _abilityCooldownTimer = 0f;
        Debug.Log("Ability cooldown finished. Ammo restored.");
        //  re-enable HUD ability icon / play VFX
    }

    // Public helpers for other scripts
    public int GetCurrentAmmo() => _currentAmmo;
    public int GetMaxAmmo() => _maxAmmo;
    public bool IsAbilityOnCooldown() => _isAbilityOnCooldown;
    public float GetAbilityCooldownRemaining() => _abilityCooldownTimer;
}