using UnityEngine;

public class SnowballProjectile : MonoBehaviour
{
    private Vector3 _velocity;
    private float _gravityScale;
    private bool _isActive;
    private static int _runnerLayer = -1;

    [Tooltip("Seconds until the projectile self-destructs if it doesn't hit anything")]
    [SerializeField] private float _lifeTime = 8f;

    private float _lifeTimer;

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
        _lifeTimer = _lifeTime;
    }

    private void Update()
    {
        if (!_isActive) return;

        SnowballDropOff();

        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;

        if (other.gameObject.layer == _runnerLayer)
        {
            OnHitRunner(other);
        }

        Destroy(gameObject);
    }

    private void OnHitRunner(Collider runnerCollider)
    {
        // TODO: do something to the runner here later
        Debug.Log("Snowball hit runner");
    }

    private void SnowballDropOff()
    {
        _velocity += Vector3.down * (_gravityScale * Time.deltaTime);
        transform.position += _velocity * Time.deltaTime;

        if (_velocity != Vector3.zero)
            transform.forward = _velocity.normalized;
    }
}