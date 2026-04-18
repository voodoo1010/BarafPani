using _Features.Abilities.Core.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Abilities.Hunter
{
    public class SnowballGunController : AbilityBase
    {
        [Header("References")]
        [SerializeField, ForceFill, Tooltip("Transform at the barrel tip where snowballs spawn")]
        private Transform _muzzlePoint;
        [SerializeField, ForceFill, Tooltip("Snowball projectile prefab")]
        private SnowballProjectile _snowballPrefab;

        [Header("Fire Settings")]
        [SerializeField, Tooltip("Shots per second (intra-ability rate limiter)"), Range(0.1f, 10f)]
        private float _fireRate = 2f;
        [SerializeField, Tooltip("Initial projectile speed in units/s"), Range(5f, 50f)]
        private float _snowballSpeed = 20f;
        [SerializeField, Tooltip("Gravity acceleration applied to projectile arc"), Range(0.1f, 50f)]
        private float _gravityScale = 9.8f;

        [Header("Ammo Settings")]
        [SerializeField, Tooltip("Shots per activation before cooldown starts"), Range(1, 100)]
        private int _maxAmmo = 10;

        private float _fireCooldownTimer;
        private int _currentAmmo;
        private Camera _mainCamera;

        protected override void OnAcquiredInternal()
        {
            _mainCamera = Camera.main;
            _currentAmmo = _maxAmmo;
            _fireCooldownTimer = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (_fireCooldownTimer > 0f)
                _fireCooldownTimer -= Time.deltaTime;

            if (CooldownRemaining <= 0f && _currentAmmo <= 0)
                _currentAmmo = _maxAmmo;
        }

        protected override bool OnActivateInternal()
        {
            if (_fireCooldownTimer > 0f) return false;
            if (_currentAmmo <= 0) return false;
            if (_muzzlePoint == null || _snowballPrefab == null || _mainCamera == null)
            {
                Debug.LogError("[SnowballGun] References not assigned.");
                return false;
            }

            Fire();
            return true;
        }

        private void Fire()
        {
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

            if (_currentAmmo <= 0)
                ConsumeActivation();
        }

        public int GetCurrentAmmo() => _currentAmmo;
        public int GetMaxAmmo() => _maxAmmo;
    }
}
