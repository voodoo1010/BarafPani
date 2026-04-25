using _Features.Player._Features.CameraView._Features.ThirdPerson.Config.Scripts;
using _Features.Player._Features.CameraView.Scripts;
using CustomInspector;
using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.ThirdPerson.Scripts
{
    public class CharacterThirdPersonCamera : CharacterCameraView
    {
        [SerializeField, ForceFill]
        private CharacterThirdPersonCameraSettings thirdPersonSettings;

        // Camera's transform is the movement reference (so WASD is camera-relative)
        protected override Transform MovementReferenceTransform => CinemachineCamera.transform;

        protected override void Awake()
        {
            base.Awake();

            // Ensure tracking target is set to the character (in case Inspector wasn't set)
            if (CinemachineCamera.Follow == null)
                CinemachineCamera.Follow = Character.transform;
            if (CinemachineCamera.LookAt == null)
                CinemachineCamera.LookAt = Character.transform;
        }

        // Cinemachine's InputAxisController handles input directly now —
        // your CharacterInput's lookAction can stop firing into ApplyLook for third-person.
        protected override void ApplyLook(float yaw, float pitch)
        {
            // Intentionally empty — Cinemachine handles look input
        }
    }
}