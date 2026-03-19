using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerTPVAnimatorView : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;

        [SerializeField] private Transform _neckBonePivot;
        [SerializeField] private Transform _LeftArmBonePivot;
        [SerializeField] private Transform _RightArmBonePivot;

        [SerializeField] private float _speedLerp = 8f;

        private float _smoothedX;
        private float _smoothedY;
        private float _smoothedCrouch;

        private FP _previousYaw;

        private static readonly int _speedXHash = Animator.StringToHash("SpeedX");
        private static readonly int _speedYHash = Animator.StringToHash("SpeedY");
        private static readonly int _crouchSpeedHash = Animator.StringToHash("CrouchSpeed");
        private static readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");
        private static readonly int _isStompingHash = Animator.StringToHash("IsStomping");
        private static readonly int _isDraggingHash = Animator.StringToHash("IsDragging");
        private static readonly int _isHoldingHash = Animator.StringToHash("IsHolding");

        public override void OnLateUpdateView()
        {
            if (VerifiedFrame.TryGet(EntityRef, out Player player) == false) return;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            Frame frame = isLocal ? PredictedFrame : VerifiedFrame;

            if (frame.Exists(EntityRef) == false) return;

            var playerPitch = Mathf.Clamp(player.LookPitch.AsFloat, -75f, 90f);

            HandleNeckBone(frame, playerPitch);
            HandleLeftArmBone(frame, playerPitch);
            HandleRightArmBone(frame, playerPitch);

            HandleMovementAnimation(frame);
        }

        private void HandleNeckBone(Frame frame, float playerPitch)
        {
            Quaternion neckPitchOffset = Quaternion.AngleAxis(playerPitch, Vector3.right);
            _neckBonePivot.localRotation *= neckPitchOffset;
        }

        private void HandleLeftArmBone(Frame frame, float playerPitch)
        {
            if (!frame.Unsafe.TryGetPointer<PlayerInventory>(EntityRef, out var playerInventory))
                return;

            bool isHolding = PlayerInventoryUtils.IsSlotSelected(playerInventory) && !PlayerInventoryUtils.IsSelectedSlotEmpty(playerInventory);

            _animator.SetBool(_isHoldingHash, isHolding);

            if (isHolding)
            {
                Quaternion leftArmPitchOffset = Quaternion.AngleAxis(playerPitch, Vector3.down);
                _LeftArmBonePivot.localRotation *= leftArmPitchOffset;
            }
        }

        private void HandleRightArmBone(Frame frame, float playerPitch)
        {
            var dragging = frame.Get<PlayerDragging>(EntityRef);

            _animator.SetBool(_isDraggingHash, dragging.IsDragging);

            if (dragging.IsDragging)
            {
                Quaternion rightArmPitchOffset = Quaternion.AngleAxis(playerPitch, Vector3.up);
                _RightArmBonePivot.localRotation *= rightArmPitchOffset;
            }
        }

        private void HandleMovementAnimation(Frame frame)
        {
            var movement = frame.Get<PlayerMovement>(EntityRef);
            var velocity = frame.Get<PhysicsBody3D>(EntityRef).Velocity;

            var horizontalVelocity = new Vector3(velocity.X.AsFloat, 0f, velocity.Z.AsFloat);

            var signedX = Vector3.Dot(horizontalVelocity, transform.right);
            var signedY = Vector3.Dot(horizontalVelocity, transform.forward);

            float horizontalMagnitude = horizontalVelocity.magnitude;

            float absX = Mathf.Abs(signedX);
            float absY = Mathf.Abs(signedY);
            float denominator = Mathf.Max(absX, absY);

            float normalizedX = (denominator > 0.001f) ? (signedX / denominator) : 0f;
            float normalizedY = (denominator > 0.001f) ? (signedY / denominator) : 0f;

            float speedScale = 0f;
            if (horizontalMagnitude > 0.05f)
            {
                speedScale = movement.IsRunning ? 1f : 0.5f;
            }

            // LOCOMOTION

            float targetX = normalizedX * speedScale;
            float targetY = normalizedY * speedScale;

            _smoothedX = Mathf.Lerp(_smoothedX, targetX, Time.deltaTime * _speedLerp);
            _smoothedY = Mathf.Lerp(_smoothedY, targetY, Time.deltaTime * _speedLerp);

            _animator.SetFloat(_speedXHash, _smoothedX);
            _animator.SetFloat(_speedYHash, _smoothedY);

            if (!movement.IsCrouching && horizontalMagnitude <= 0.05f)
            {
                if (VerifiedFrame.TryGet(EntityRef, out Player player) == false) return;

                FP deltaYaw = FPMath.Abs(player.LookYaw - _previousYaw);

                _animator.SetBool(_isStompingHash, deltaYaw > FP._0);
                
                _previousYaw = player.LookYaw;
            }
            else
            {
                _animator.SetBool(_isStompingHash, false);
            }

            // CROUCH LOCOMOTION

            _animator.SetBool(_isCrouchingHash, movement.IsCrouching);

            if (!movement.IsCrouching)
            {
                _smoothedCrouch = 0f;
                return;
            }

            float targetCrouch = (horizontalMagnitude > 0.05f) ? 1f : 0f;

            _smoothedCrouch = Mathf.Lerp(_smoothedCrouch, targetCrouch, Time.deltaTime * _speedLerp);

            _animator.SetFloat(_crouchSpeedHash, _smoothedCrouch);
        }
    }
}
