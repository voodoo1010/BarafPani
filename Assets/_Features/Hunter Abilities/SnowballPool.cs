using System.Collections.Generic;
using UnityEngine;

public class SnowballPool : MonoBehaviour
{
    public static SnowballPool Instance { get; private set; }

    [SerializeField] private SnowballProjectile _snowballPrefab;
    [SerializeField] private int _initialPoolSize = 20;

    private readonly Queue<SnowballProjectile> _pool = new Queue<SnowballProjectile>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < _initialPoolSize; i++)
        {
            SnowballProjectile snowball = Instantiate(_snowballPrefab, transform);
            snowball.gameObject.SetActive(false);
            _pool.Enqueue(snowball);
        }
    }


    public SnowballProjectile GetFromPool(Vector3 position, Quaternion rotation)
    {
        SnowballProjectile snowball;

        if (_pool.Count > 0)
        {
            snowball = _pool.Dequeue();
        }
        else
        {
            snowball = Instantiate(_snowballPrefab, transform);
        }

        snowball.transform.SetPositionAndRotation(position, rotation);
        snowball.gameObject.SetActive(true);
        return snowball;
    }

    public void ReturnToPool(SnowballProjectile snowball)
    {
        snowball.gameObject.SetActive(false);
        snowball.transform.SetParent(transform);
        _pool.Enqueue(snowball);
    }
}