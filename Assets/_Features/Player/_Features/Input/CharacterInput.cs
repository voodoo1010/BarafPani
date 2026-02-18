using _Features.Player.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Player._Features.Input
{
    [RequireComponent(typeof(Character))]
    public class CharacterInput : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference sprintAction;
        [SerializeField] private InputActionReference crouchAction;
        [SerializeField] private InputActionReference grabAction;
        [SerializeField] private InputActionReference jumpAction;

        private Character _character;

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void OnEnable()
        {
            EnableAction(moveAction, OnMove);
            EnableAction(sprintAction, OnSprint);
            EnableAction(crouchAction, OnCrouch);
            EnableAction(grabAction, OnGrab);
            EnableAction(jumpAction, OnJump);
        }

        private void OnDisable()
        {
            DisableAction(moveAction, OnMove);
            DisableAction(sprintAction, OnSprint);
            DisableAction(crouchAction, OnCrouch);
            DisableAction(grabAction, OnGrab);
            DisableAction(jumpAction, OnJump);
        }

        private void EnableAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
        {
            if (!actionRef) return;
            actionRef.action.performed += callback;
            actionRef.action.canceled += callback;
            actionRef.action.Enable();
        }

        private void DisableAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
        {
            if (!actionRef) return;
            actionRef.action.performed -= callback;
            actionRef.action.canceled -= callback;
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _character.RaiseMoveInput(ctx.ReadValue<Vector2>());
        }

        private void OnSprint(InputAction.CallbackContext ctx)
        {
            _character.RaiseSprintInput(ctx.performed);
        }

        private void OnCrouch(InputAction.CallbackContext ctx)
        {
            _character.RaiseCrouchInput(ctx.performed);
        }

        private void OnGrab(InputAction.CallbackContext ctx)
        {
            _character.RaiseGrabInput(ctx.performed);
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            _character.RaiseJumpInput(ctx.performed);
        }
    }
}