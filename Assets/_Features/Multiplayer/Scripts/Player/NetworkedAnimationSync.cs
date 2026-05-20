using Unity.Netcode;
using UnityEngine;
using _Features.Player.Scripts;
using _Features.Player._Features.Walk.Scripts;
using _Features.Player._Features.Crouch.Scripts;
using _Features.Player._Features.Jump.Scripts;

public class NetworkedAnimationSync : NetworkBehaviour
{
    private static readonly int BlendHash = Animator.StringToHash("Blend");
    private static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("JumpTrigger");

    [SerializeField, Range(0.1f, 50f)]
    private float blendSmoothing = 10f;

    private NetworkVariable<float> _netBlend = new(0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private NetworkVariable<bool> _netIsCrouching = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private NetworkVariable<bool> _netIsGrounded = new(true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private NetworkVariable<int> _netJumpTriggerCount = new(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private Animator _animator;
    private Character _character;
    private IMovementDataProvider _movementDataProvider;
    private ICrouchDataProvider _crouchDataProvider;
    private CharacterJump _characterJump;

    private float _currentBlend;
    private int _lastJumpTriggerCount;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _character = GetComponentInParent<Character>();
        _movementDataProvider = GetComponentInParent<CharacterWalk>();
        _crouchDataProvider = GetComponentInParent<CharacterCrouch>();
        _characterJump = GetComponentInParent<CharacterJump>();
    }

    public override void OnNetworkSpawn()
    {
        _lastJumpTriggerCount = _netJumpTriggerCount.Value;

        if (IsOwner)
        {
            if (_characterJump != null)
                _characterJump.JumpStarted += HandleJumpStarted;
        }
        else
        {
            var localController = GetComponent<RunnerAnimationController>();
            if (localController != null)
                localController.enabled = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && _characterJump != null)
            _characterJump.JumpStarted -= HandleJumpStarted;
    }

    private void Update()
    {
        if (IsOwner)
            WriteNetworkState();
        else
            ReadNetworkState();
    }


    private void WriteNetworkState()
    {
        float targetBlend = _movementDataProvider != null
            ? Mathf.Clamp01(_movementDataProvider.MoveInput.magnitude)
            : 0f;
        float smoothFactor = 1f - Mathf.Exp(-blendSmoothing * Time.deltaTime);
        _currentBlend = Mathf.Lerp(_currentBlend, targetBlend, smoothFactor);
        if (targetBlend < 0.01f && _currentBlend < 0.001f)
            _currentBlend = 0f;
        _netBlend.Value = _currentBlend;

        _netIsCrouching.Value = _crouchDataProvider != null && _crouchDataProvider.IsCrouching;
        _netIsGrounded.Value = _character != null && _character.IsGrounded;
    }

    private void HandleJumpStarted()
    {
        _netJumpTriggerCount.Value++;
    }


    private void ReadNetworkState()
    {
        float smoothFactor = 1f - Mathf.Exp(-blendSmoothing * Time.deltaTime);
        _currentBlend = Mathf.Lerp(_currentBlend, _netBlend.Value, smoothFactor);
        _animator.SetFloat(BlendHash, _currentBlend);

        _animator.SetBool(IsCrouchingHash, _netIsCrouching.Value);
        _animator.SetBool(IsGroundedHash, _netIsGrounded.Value);

        if (_netJumpTriggerCount.Value != _lastJumpTriggerCount)
        {
            _animator.SetTrigger(JumpTriggerHash);
            _lastJumpTriggerCount = _netJumpTriggerCount.Value;
        }
    }
}