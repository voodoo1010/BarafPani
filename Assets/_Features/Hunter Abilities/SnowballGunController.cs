using _Features.Abilities.Core.Scripts;
using UnityEngine;

namespace _Features.Abilities.Hunter
{
    public class SnowballGunController : AbilityBase
    {
        [Header("References")]
        [SerializeField] private Transform _muzzlePoint;
        [SerializeField] private SnowballProjectile _snowballPrefab;

        [Header("Fire Settings")]
        [Tooltip("Shots per second while held (intra-ability rate limiter).")]
        [SerializeField] private float _fireRate = 2f;

        [SerializeField] private float _snowballSpeed = 20f;
        [SerializeField] private float _gravityScale = 9.8f;

        [Header("Ammo Settings")]
        [Tooltip("Shots per activation before the ammo is exhausted and the cooldown (from AbilityData) starts.")]
        [SerializeField] private int _maxAmmo = 10;

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
