using UnityEngine;

namespace Quantum
{
    public class PlayerAnimatorView : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;

        [SerializeField] private Transform _neckBonePivot;
        [SerializeField] private Transform _RightArmBonePivot;

        [SerializeField] private float speedLerp = 8f;

        private float _locomotionSmoothed;
        private float _locomotionCrouchSmoothed;

        private static readonly int _speedHash = Animator.StringToHash("Speed");
        private static readonly int _crouchSpeedHash = Animator.StringToHash("CrouchSpeed");
        private static readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");

        public override void OnLateUpdateView()
        {
            if (VerifiedFrame.TryGet(EntityRef, out Player player) == false) return;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            Frame frame = isLocal ? PredictedFrame : VerifiedFrame;

            if (frame.Exists(EntityRef) == false) return;

            var playerPitch = Mathf.Clamp(player.LookPitch.AsFloat, -75f, 90f);

            Quaternion neckPitchOffset = Quaternion.AngleAxis(playerPitch, Vector3.right);
            _neckBonePivot.localRotation *= neckPitchOffset;

            // DRAGGING

            var dragging = frame.Get<PlayerDragging>(EntityRef);

            _animator.SetBool("IsDragging", dragging.IsDragging);

            if (dragging.IsDragging)
            {
                Quaternion rightArmPitchOffset = Quaternion.AngleAxis(playerPitch, Vector3.up);
                _RightArmBonePivot.localRotation *= rightArmPitchOffset;
            }

            // MOVEMENT

            var movement = frame.Get<PlayerMovement>(EntityRef);
            var velocity = frame.Get<PhysicsBody3D>(EntityRef).Velocity;

            var horizontalVelocity = new Vector3(velocity.X.AsFloat, 0f, velocity.Z.AsFloat);
            var signedSpeed = Vector3.Dot(horizontalVelocity, transform.forward);
            float targetSpeed = 0f;

            // LOCOMOTION

            if (Mathf.Abs(signedSpeed) > 0.05f)
            {
                targetSpeed = movement.IsRunning
                    ? 1f
                    : 0.5f;
                targetSpeed *= Mathf.Sign(signedSpeed);
            }

            _locomotionSmoothed = Mathf.Lerp(_locomotionSmoothed, targetSpeed, Time.deltaTime * speedLerp);

            _animator.SetFloat(_speedHash, _locomotionSmoothed);

            // CROUCH LOCOMOTION

            _animator.SetBool(_isCrouchingHash, movement.IsCrouching);

            if (!movement.IsCrouching)
            {
                _locomotionCrouchSmoothed = 0f;
                return;
            }

            targetSpeed = Mathf.Abs(signedSpeed) > 0.05f
                ? 1f
                : 0f;
            targetSpeed *= Mathf.Sign(signedSpeed);

            _locomotionCrouchSmoothed = Mathf.Lerp(_locomotionCrouchSmoothed, targetSpeed, Time.deltaTime * speedLerp);

            _animator.SetFloat(_crouchSpeedHash, _locomotionCrouchSmoothed);
        }
    }
}
