using UnityEngine;

namespace Quantum
{
    public class PlayerAnimatorView : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int CrouchSpeedHash = Animator.StringToHash("CrouchSpeed");
        private static readonly int IsCrouchHash = Animator.StringToHash("IsCrouching");

        public override void OnActivate(Frame frame)
        {
            base.OnActivate(frame);
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);
        }

        public override void OnLateUpdateView()
        {
            if (_animator == null)
                return;

            if (VerifiedFrame.TryGet(EntityRef, out Player player) == false)
                return;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            Frame f = isLocal ? PredictedFrame : VerifiedFrame;

            if (f.Exists(EntityRef) == false)
                return;

            var m = f.Get<PlayerMovement>(EntityRef);
            var body = f.Get<PhysicsBody3D>(EntityRef);

            var v = body.Velocity;
            float hs = Mathf.Sqrt(v.X.AsFloat * v.X.AsFloat + v.Z.AsFloat * v.Z.AsFloat);

            // --- LOCOMOTION (DISCRETE) ---
            float speedParam = 0f;

            if (hs > 0.05f)
            {
                speedParam = m.IsRunning ? 1f : 0.5f;
            }

            _animator.SetFloat(SpeedHash, speedParam);

            // --- CROUCH LOCOMOTION ---
            float crouch = m.CrouchSpeed.AsFloat;
            float crouch01 = (crouch > 0.0001f) ? Mathf.Clamp01(hs / crouch) : 0f;

            _animator.SetFloat(CrouchSpeedHash, crouch01);
            _animator.SetBool(IsCrouchHash, m.IsCrouching);
        }
    }
}
