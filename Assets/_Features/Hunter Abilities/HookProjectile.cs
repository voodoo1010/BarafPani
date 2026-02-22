using System;
using UnityEngine;

public class HookProjectile : MonoBehaviour
{
    private enum HookState { Launching, Pulling, Retracting }

    private Transform _origin;
    private Vector3 _direction;
    private float _hookSpeed;
    private float _pullSpeed;
    private float _maxDistance;
    private LayerMask _runnerLayer;
    private Material _lineMaterial;
    private Action _onFinished;

    private HookState _state = HookState.Launching;
    private Transform _caughtTarget;
    private LineRenderer _lineRenderer;
    private float _distanceTravelled;

    public void Initialize(
        Transform origin,
        Vector3 direction,
        float hookSpeed,
        float pullSpeed,
        float maxDistance,
        LayerMask runnerLayer,
        Material lineMaterial,
        Action onFinished)
    {
        _origin = origin;
        _direction = direction.normalized;
        _hookSpeed = hookSpeed;
        _pullSpeed = pullSpeed;
        _maxDistance = maxDistance;
        _runnerLayer = runnerLayer;
        _lineMaterial = lineMaterial;
        _onFinished = onFinished;

        transform.position = origin.position;
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.material = _lineMaterial;
    }

    private void Update()
    {
        switch (_state)
        {
            case HookState.Launching: HandleLaunching(); break;
            case HookState.Pulling: HandlePulling(); break;
            case HookState.Retracting: HandleRetracting(); break;
        }

        UpdateLineRenderer();
    }

    private void HandleLaunching()
    {
        Vector3 movement = _direction * (_hookSpeed * Time.deltaTime);
        transform.position += movement;
        _distanceTravelled += movement.magnitude;

        if (_distanceTravelled >= _maxDistance)
        {
            _state = HookState.Retracting;
            return;
        }

        Vector3 toHookTip = transform.position - _origin.position;
        float currentLength = toHookTip.magnitude;

        if (Physics.Raycast(_origin.position, toHookTip.normalized, out RaycastHit hit, currentLength))
        {
            if ((_runnerLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                _caughtTarget = hit.collider.transform;
                _state = HookState.Pulling;
            }
            else
            {
                _state = HookState.Retracting;
            }
        }
    }

    private void HandlePulling()
    {
        if (_caughtTarget == null)
        {
            _state = HookState.Retracting;
            return;
        }

        _caughtTarget.position = Vector3.MoveTowards(
            _caughtTarget.position,
            _origin.position,
            _pullSpeed * Time.deltaTime
        );

        transform.position = _caughtTarget.position;

        if (Vector3.Distance(_caughtTarget.position, _origin.position) < 1.5f)
        {
            Finish();
        }
    }

    private void HandleRetracting()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            _origin.position,
            _hookSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, _origin.position) < 0.1f)
        {
            Finish();
        }
    }

    private void UpdateLineRenderer()
    {
        _lineRenderer.SetPosition(0, _origin.position);
        _lineRenderer.SetPosition(1, transform.position);
    }

    private void Finish()
    {
        _onFinished?.Invoke();
        Destroy(gameObject);
    }
}


