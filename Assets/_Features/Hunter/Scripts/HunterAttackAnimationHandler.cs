using UnityEngine;

namespace _Features.Hunter.Scripts
{
    [RequireComponent(typeof(Animator))]
    public class HunterAttackAnimationHandler : MonoBehaviour
    {
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int AttackIndex = Animator.StringToHash("AttackIndex");

        private const int AttackVariantCount = 2;

        private Animator _animator;
        private HunterCharacterInput _hunterInput;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _hunterInput = GetComponentInParent<HunterCharacterInput>();
        }

        private void OnEnable()
        {
            _hunterInput.OnAttackInput += HandleAttack;
        }

        private void OnDisable()
        {
            _hunterInput.OnAttackInput -= HandleAttack;
        }

        private void HandleAttack()
        {
            _animator.SetInteger(AttackIndex, Random.Range(0, AttackVariantCount));
            _animator.SetTrigger(Attack);
        }
    }
}
