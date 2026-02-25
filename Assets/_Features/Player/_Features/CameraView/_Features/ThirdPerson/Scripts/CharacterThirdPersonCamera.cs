using _Features.Player._Features.CameraView._Features.ThirdPerson.Config.Scripts;
using _Features.Player._Features.CameraView.Scripts;
using UnityEngine;

namespace _Features.Player._Features.CameraView._Features.ThirdPerson.Scripts
{
    public class CharacterThirdPersonCamera : CharacterCameraView
    {
        [SerializeField] private CharacterThirdPersonCameraSettings thirdPersonSettings;

        private Transform _pivot;
        private float _yaw;
        private float _pitch;

        protected override Transform MovementReferenceTransform => _pivot;

        protected override void Awake()
        {
            base.Awake();

            _pivot = new GameObject("CameraPivot_ThirdPerson").transform;
            _pivot.SetParent(Character.transform);
            _pivot.localPosition = Vector3.zero;

            CinemachineCamera.Follow = _pivot;
            CinemachineCamera.LookAt = Character.transform;
        }

        private void OnDestroy()
        {
            if (_pivot) Destroy(_pivot.gameObject);
        }

        protected override void ApplyLook(float yaw, float pitch)
        {
            _yaw += yaw;
            _pitch -= pitch;
            _pitch = Mathf.Clamp(_pitch, thirdPersonSettings.PitchClampMin, thirdPersonSettings.PitchClampMax);

            _pivot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }
}