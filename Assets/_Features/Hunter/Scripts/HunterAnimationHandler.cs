using _Features.Player._Features.Walk.Scripts;
using _Features.Player.Scripts;
using UnityEngine;

namespace _Features.Hunter.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class HunterAnimationHandler : MonoBehaviour
    {
        private static readonly int BlendX = Animator.StringToHash("BlendX");
        private static readonly int BlendY = Animator.StringToHash("BlendY");

        [SerializeField, Range(0.1f, 50f), Tooltip("Exponential blend speed for animation transitions. Higher = snappier.")]
        private float blendSmoothing = 10f;

        private Animator _animator;
        private IMovementDataProvider _movementDataProvider;
        private Vector2 _smoothedBlend;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _movementDataProvider = GetComponentInParent<CharacterWalk>();
        }

        private void Update()
        {
            Vector2 input = _movementDataProvider.MoveInput;

            float smoothFactor = 1f - Mathf.Exp(-blendSmoothing * Time.deltaTime);
            _smoothedBlend = Vector2.Lerp(_smoothedBlend, input, smoothFactor);

            if (input.sqrMagnitude < 0.01f && _smoothedBlend.sqrMagnitude < 0.001f)
                _smoothedBlend = Vector2.zero;

            _animator.SetFloat(BlendX, _smoothedBlend.x);
            _animator.SetFloat(BlendY, _smoothedBlend.y);
        }
    }
}