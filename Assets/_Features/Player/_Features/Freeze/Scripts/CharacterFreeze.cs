using _Features.Player._Features.Freeze.Config.Scripts;
using _Features.Player._Features.Walk.Scripts;
using _Features.Player.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Player._Features.Freeze.Scripts
{
    [RequireComponent(typeof(CharacterWalk))]
    public class CharacterFreeze : CharacterFeature, IFreezable
    {
        [HorizontalLine("Configuration", 1, FixedColor.Cyan)]
        [SerializeField, ForceFill, Tooltip("ScriptableObject with freeze configuration (thresholds, decay, slowdown curve)")]
        private CharacterFreezeSettings characterFreezeSettings;

        [HorizontalLine("Runtime", 1, FixedColor.Black)]
        [SerializeField, ReadOnly, Tooltip("Current freeze factor — exposed for debugging")]
        private float _freezeFactor;

        [SerializeField, ReadOnly, Tooltip("True while the character is hard-locked at max factor")]
        private bool _isHardFrozen;

        private CharacterWalk _characterWalk;

        private float _freezeTimer;
        private Vector3 _frozenPosition;

        private Rigidbody _victimRb;
        private RigidbodyConstraints _originalConstraints;
        private bool _hadRigidbodyCached;

        private CharacterController _victimCc;

        public float FreezeFactor => _freezeFactor;
        public bool IsHardFrozen => _isHardFrozen;

        protected override void Awake()
        {
            base.Awake();
            _characterWalk = GetComponent<CharacterWalk>();
        }

        private void OnDisable()
        {
            if (_isHardFrozen)
                Unfreeze();

            if (_characterWalk != null)
                _characterWalk.FreezeSpeedMultiplier = 1f;

            _freezeFactor = 0f;
        }

        private void Update()
        {
            if (_isHardFrozen)
            {
                EnforceHardFreeze();
                TickHardFreezeTimer();
                return;
            }

            DecayFactor();
            ApplySlowdown();

            if (_freezeFactor >= characterFreezeSettings.MaxFreezeFactor)
                EnterHardFreeze();
        }

        public void ApplyFreeze(float amount)
        {
            if (_isHardFrozen) return;
            if (amount <= 0f) return;

            _freezeFactor = Mathf.Min(_freezeFactor + amount, characterFreezeSettings.MaxFreezeFactor);
        }

        private void DecayFactor()
        {
            if (_freezeFactor <= 0f) return;

            _freezeFactor = Mathf.Max(0f, _freezeFactor - characterFreezeSettings.FreezeDecayPerSecond * Time.deltaTime);
        }

        private void ApplySlowdown()
        {
            float normalized = _freezeFactor / characterFreezeSettings.MaxFreezeFactor;
            float t = characterFreezeSettings.SlowCurve.Evaluate(normalized);
            _characterWalk.FreezeSpeedMultiplier = Mathf.Lerp(1f, characterFreezeSettings.MinSlowMultiplier, t);
        }

        private void EnterHardFreeze()
        {
            _isHardFrozen = true;
            _freezeTimer = characterFreezeSettings.FullFreezeDuration;
            _frozenPosition = transform.position;

            _characterWalk.FreezeSpeedMultiplier = 0f;

            _victimCc = Character.CharacterControllerUnityComponent;
            if (_victimCc != null)
                _victimCc.enabled = false;

            _victimRb = GetComponent<Rigidbody>();
            if (_victimRb != null)
            {
                _hadRigidbodyCached = true;
                _originalConstraints = _victimRb.constraints;
                _victimRb.linearVelocity = Vector3.zero;
                _victimRb.angularVelocity = Vector3.zero;
                _victimRb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }

        private void EnforceHardFreeze()
        {
            transform.position = _frozenPosition;

            if (_victimRb != null)
            {
                _victimRb.linearVelocity = Vector3.zero;
                _victimRb.angularVelocity = Vector3.zero;
            }
        }

        private void TickHardFreezeTimer()
        {
            _freezeTimer -= Time.deltaTime;
            if (_freezeTimer <= 0f)
                Unfreeze();
        }

        private void Unfreeze()
        {
            if (_hadRigidbodyCached && _victimRb != null)
            {
                _victimRb.constraints = _originalConstraints;
            }
            _victimRb = null;
            _hadRigidbodyCached = false;

            if (_victimCc != null)
            {
                _victimCc.enabled = true;
                _victimCc = null;
            }

            _isHardFrozen = false;
            _freezeFactor = 0f;
            _characterWalk.FreezeSpeedMultiplier = 1f;
        }
    }
}
