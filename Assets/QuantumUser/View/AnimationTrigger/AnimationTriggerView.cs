using UnityEngine;

namespace Quantum
{
    public unsafe class AnimationTriggerView : QuantumEntityViewComponent
    {
        [SerializeField] private Animator _animator;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventAnimationTriggered>(this, OnEventAnimationTriggered);
        }

        private void OnEventAnimationTriggered(EventAnimationTriggered e)
        {
            if (e.Entity != EntityRef) return;

            var frame = QuantumRunner.Default.Game.Frames.Predicted;
            if (frame == null) return;

            var animationTrigger = frame.Unsafe.GetPointer<AnimationTrigger>(EntityRef);

            _animator.SetBool("IsToggled", animationTrigger->IsToggled);
        }
    }
}
