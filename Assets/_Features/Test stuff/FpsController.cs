using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class FpsMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSmoothSpeed = 10f;
    [SerializeField] private bool snapWhenMoving = true;

    private CharacterController _controller;
    private TestingControls _controls;
    private Vector2 _moveInput;
    private float _yVelocity;

    private const float Gravity = -9.81f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _controls = new TestingControls();

        _controls.Test.movement.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.Test.movement.canceled += _ => _moveInput = Vector2.zero;
    }

    private void OnEnable() => _controls.Enable();
    private void OnDisable() => _controls.Disable();

    private void Update()
    {
        HandleMovement();
    }

    private void LateUpdate()
    {
        RotateBodyToLookDirection();
    }

    private void HandleMovement()
    {
        Vector3 right = transform.right;
        Vector3 forward = transform.forward;

        Vector3 move = right * _moveInput.x + forward * _moveInput.y;
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (_controller.isGrounded && _yVelocity < 0f)
            _yVelocity = -2f;

        _yVelocity += Gravity * Time.deltaTime;

        Vector3 velocity = move * moveSpeed + Vector3.up * _yVelocity;
        _controller.Move(velocity * Time.deltaTime);
    }

    private void RotateBodyToLookDirection()
    {
        Transform cam = cameraTransform != null ? cameraTransform : Camera.main?.transform;
        if (cam == null) return;

        Vector3 look = cam.forward;
        look.y = 0f;
        if (look.sqrMagnitude < 0.001f) return;
        look.Normalize();

        float targetYaw = Quaternion.LookRotation(look).eulerAngles.y;
        float currentYaw = transform.eulerAngles.y;

        float t = rotationSmoothSpeed * Time.deltaTime;
        float newYaw = (snapWhenMoving && _moveInput.sqrMagnitude > 0.001f)
            ? targetYaw
            : Mathf.LerpAngle(currentYaw, targetYaw, t);

        transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
    }

    private void OnDestroy()
    {
        if (_controls != null)
        {
            _controls.Test.movement.performed -= ctx => _moveInput = ctx.ReadValue<Vector2>();
            _controls.Test.movement.canceled -= _ => _moveInput = Vector2.zero;
            _controls.Dispose();
        }
    }
}