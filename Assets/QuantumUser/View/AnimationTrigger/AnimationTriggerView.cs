using UnityEngine;

namespace Quantum
{
    public unsafe class AnimationTriggerView : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventAnimationTriggerEnter>(this, OnEventAnimationTriggerEnter);
            QuantumEvent.Subscribe<EventAnimationTriggerExit>(this, OnEventAnimationTriggerExit);
        }

        private void OnEventAnimationTriggerEnter(EventAnimationTriggerEnter e)
        {
            if (e.Entity != EntityRef) return;

            var frame = QuantumRunner.Default.Game.Frames.Predicted;
            if (frame == null) return;

            var animationTrigger = frame.Unsafe.GetPointer<AnimationTrigger>(EntityRef);

            _animator.SetBool("IsToggled", animationTrigger->IsToggled);
        }

        private void OnEventAnimationTriggerExit(EventAnimationTriggerExit e)
        {
            if (e.Entity != EntityRef) return;

            var frame = QuantumRunner.Default.Game.Frames.Predicted;
            if (frame == null) return;

            var animationTrigger = frame.Unsafe.GetPointer<AnimationTrigger>(EntityRef);

            _animator.SetBool("IsToggled", animationTrigger->IsToggled);
        }
    }
}
