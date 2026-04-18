using _Features.Player.Scripts;
using CustomInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Features.Player._Features.Input
{
    [RequireComponent(typeof(Character))]
    public class CharacterInput : MonoBehaviour
    {
        [HorizontalLine("Input Actions", 1, FixedColor.Cyan)]
        [SerializeField, ForceFill, Tooltip("Player movement input (WASD/stick)")]
        private InputActionReference moveAction;
        [SerializeField, ForceFill, Tooltip("Camera look input (mouse/stick)")]
        private InputActionReference lookAction;
        [SerializeField, Tooltip("Sprint toggle input")]
        private InputActionReference sprintAction;
        [SerializeField, Tooltip("Crouch toggle input")]
        private InputActionReference crouchAction;
        [SerializeField, Tooltip("Grab interaction input")]
        private InputActionReference grabAction;
        [SerializeField, Tooltip("Jump input")]
        private InputActionReference jumpAction;

        private Character _character;

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        protected virtual void OnEnable()
        {
            EnableAction(moveAction, OnMove);
            EnableAction(lookAction, OnLook);
            EnableAction(sprintAction, OnSprint);
            EnableAction(crouchAction, OnCrouch);
            EnableAction(grabAction, OnGrab);
            EnableAction(jumpAction, OnJump);
        }

        protected virtual void OnDisable()
        {
            DisableAction(moveAction, OnMove);
            DisableAction(lookAction, OnLook);
            DisableAction(sprintAction, OnSprint);
            DisableAction(crouchAction, OnCrouch);
            DisableAction(grabAction, OnGrab);
            DisableAction(jumpAction, OnJump);
        }

        protected void EnableAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
        {
            if (!actionRef) return;
            actionRef.action.performed += callback;
            actionRef.action.canceled += callback;
            actionRef.action.Enable();
        }

        protected void DisableAction(InputActionReference actionRef, System.Action<InputAction.CallbackContext> callback)
        {
            if (!actionRef) return;
            actionRef.action.performed -= callback;
            actionRef.action.canceled -= callback;
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _character.RaiseMoveInput(ctx.ReadValue<Vector2>());
        }

        private void OnLook(InputAction.CallbackContext ctx)
        {
            _character.RaiseLookInput(ctx.ReadValue<Vector2>());
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