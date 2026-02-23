using UnityEngine;
using UnityEngine.InputSystem;

public class SnowballGunController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _muzzlePoint;

    [Header("Settings")]
    [Tooltip("Shots per second")]
    [SerializeField] private float _fireRate = 2f;

    [Tooltip("Speed of the snowball")]
    [SerializeField] private float _snowballSpeed = 20f;

    [Tooltip("Gravity of the snowball")]
    [SerializeField] private float _gravityScale = 9.8f;

    private TestingControls _controls;
    private float _fireCooldown;
    private Camera _mainCamera;

    private void Awake()
    {
        _controls = new TestingControls();
        _mainCamera = Camera.main;
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
        if (_fireCooldown > 0f)
            _fireCooldown -= Time.deltaTime;
    }

    private void OnFiregunPerformed(InputAction.CallbackContext context)
    {
        Fire();
    }

    private void Fire()
    {
        if (_fireCooldown > 0f) return;

        Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, 500f)
            ? hit.point
            : ray.GetPoint(500f);

        Vector3 fireDirection = (targetPoint - _muzzlePoint.position).normalized;

        SnowballProjectile snowball = SnowballPool.Instance.GetFromPool(
            _muzzlePoint.position,
            Quaternion.LookRotation(fireDirection)
        );

        snowball.Launch(fireDirection, _snowballSpeed, _gravityScale);

        _fireCooldown = 1f / _fireRate;
    }
}