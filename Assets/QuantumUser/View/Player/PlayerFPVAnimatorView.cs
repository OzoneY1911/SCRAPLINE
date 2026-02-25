using UnityEngine;

namespace Quantum
{
    public class PlayerFPVAnimatorView : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;

        public override void OnLateUpdateView()
        {
            if (VerifiedFrame.TryGet(EntityRef, out Player player) == false) return;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            Frame frame = isLocal ? PredictedFrame : VerifiedFrame;

            if (frame.Exists(EntityRef) == false) return;

            var dragging = frame.Get<PlayerDragging>(EntityRef);

            _animator.SetBool("IsDragging", dragging.IsDragging);
        }
    }
}
