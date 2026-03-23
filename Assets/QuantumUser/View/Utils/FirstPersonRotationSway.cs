using UnityEngine;

namespace Quantum
{
    public unsafe class FirstPersonRotationSway : QuantumEntityViewComponent
    {
        [Header("Rotation Sway Settings")]
        [SerializeField] private float _swaySpeed = 5;
        [SerializeField] private float _swayStrength = 1;
        [SerializeField] private float _maxSwayAngle = 10f;

        public override void OnLateUpdateView()
        {
            if (VerifiedFrame.TryGet(EntityRef, out Player player) == false) return;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            if (!isLocal) return;

            Frame frame = PredictedFrame;
            if (frame.Exists(EntityRef) == false) return;

            Vector2 lookRotationDelta = frame.GetPlayerInput(player.PlayerRef)->LookRotationDelta.ToUnityVector2();

            float mouseX = Mathf.Clamp(lookRotationDelta.x * _swayStrength, -_maxSwayAngle, _maxSwayAngle);
            float mouseY = Mathf.Clamp(lookRotationDelta.y * _swayStrength, -_maxSwayAngle, _maxSwayAngle);

            Quaternion targetRotation =
                Quaternion.AngleAxis(mouseY, Vector3.up) *
                Quaternion.AngleAxis(mouseX, Vector3.right);

            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, _swaySpeed * Time.deltaTime);
        }
    }
}