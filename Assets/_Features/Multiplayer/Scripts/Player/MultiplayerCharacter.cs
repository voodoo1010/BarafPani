using _Features.Player._Features.CameraView._Features.ThirdPerson.Scripts;
using _Features.Player._Features.Crouch.Scripts;
using _Features.Player._Features.Grab.Scripts;
using _Features.Player._Features.Input;
using _Features.Player._Features.Jump.Scripts;
using _Features.Player._Features.Sprint.Scripts;
using _Features.Player._Features.Walk.Scripts;
using _Features.Player.Scripts;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerCharacter : NetworkBehaviour
{
    private Character character;
    private CharacterInput characterInput;
    private CharacterWalk characterWalk;
    private CharacterCrouch characterCrouch;
    private CharacterGrab characterGrab;
    private CharacterJump characterJump;
    private CharacterThirdPersonCamera characterTPC;
    private CharacterSprint characterSprint;
    private CharacterController characterController;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject cineMachine;
    [SerializeField] private AudioListener audioListener;


    private void Awake()
    {
        Initialize();
    }
    private void Initialize()
    {
        character = GetComponent<Character>();
        characterInput = GetComponent<CharacterInput>();
        characterWalk = GetComponent<CharacterWalk>();
        characterCrouch = GetComponent<CharacterCrouch>();
        characterGrab = GetComponent<CharacterGrab>();
        characterJump = GetComponent<CharacterJump>();
        characterTPC = GetComponent<CharacterThirdPersonCamera>();
        characterSprint = GetComponent<CharacterSprint>();
        characterController = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"MultiplayerCharacter.OnNetworkSpawn | IsOwner: {IsOwner} | IsServer: {IsServer} | IsClient: {IsClient} | OwnerClientId: {OwnerClientId} | LocalClientId: {NetworkManager.Singleton.LocalClientId}");
        bool isOwner = IsOwner;

        character.enabled = isOwner;
        characterInput.enabled = isOwner;
        characterWalk.enabled = isOwner;
        characterSprint.enabled = isOwner;
        characterCrouch.enabled = isOwner;
        characterGrab.enabled = isOwner;
        characterJump.enabled = isOwner;
        characterTPC.enabled = isOwner;

        characterController.enabled = isOwner;

        playerCamera.gameObject.SetActive(isOwner);
        cineMachine.SetActive(isOwner);

        if (audioListener != null)
            audioListener.enabled = isOwner;

        if (isOwner)
        {
            character.CameraTransform = playerCamera.transform;
        }
    }
}