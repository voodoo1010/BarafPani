using System.Diagnostics;
using UnityEngine;

public class SnowballProjectile : MonoBehaviour
{
    private Vector3 _velocity;
    private float _gravityScale;
    private bool _isActive;
    private static int _runnerLayer = -1;

    private void Awake()
    {
        if (_runnerLayer == -1)
            _runnerLayer = LayerMask.NameToLayer("Runner");
    }

    public void Launch(Vector3 direction, float speed, float gravityScale)
    {
        _velocity = direction.normalized * speed;
        _gravityScale = gravityScale;
        _isActive = true;
    }

    private void Update()
    {
        if (!_isActive) return;

        SnowballDropOff();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;

        if (other.gameObject.layer == _runnerLayer)
        {
            OnHitRunner(other);
        }

        Deactivate();
    }

    private void OnHitRunner(Collider runnerCollider)
    {
        // TODO: do something to the runner here later
        UnityEngine.Debug.Log("Snowball hit runner");
    }

    private void Deactivate()
    {
        _isActive = false;
        _velocity = Vector3.zero;
        SnowballPool.Instance.ReturnToPool(this);
    }

    private void SnowballDropOff()
    {
        _velocity += Vector3.down * (_gravityScale * Time.deltaTime);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity != Vector3.zero)
            transform.forward = _velocity.normalized;
    }
}