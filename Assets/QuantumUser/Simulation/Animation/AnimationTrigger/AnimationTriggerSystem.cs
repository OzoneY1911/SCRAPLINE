namespace Quantum
{
    public unsafe class AnimationTriggerSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D
    {
        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<AnimationTrigger>(triggerInfo.Entity, out AnimationTrigger* animationTrigger)) return;

            if (frame.Has<Player>(triggerInfo.Other))
            {
                animationTrigger->InTriggerCount++;

                if (animationTrigger->InTriggerCount > 1) return;

                animationTrigger->IsToggled = true;

                SetTargets(frame, animationTrigger, true);
            }
        }

        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<AnimationTrigger>(triggerInfo.Entity, out AnimationTrigger* animationTrigger)) return;

            if (frame.Has<Player>(triggerInfo.Other))
            {
                animationTrigger->InTriggerCount--;

                if (animationTrigger->InTriggerCount != 0) return;

                animationTrigger->IsToggled = false;

                SetTargets(frame, animationTrigger, false);
            }
        }

        private void SetTargets(Frame frame, AnimationTrigger* animationTrigger, bool active)
        {
            var animatedTargets = frame.ResolveHashSet<EntityRef>(animationTrigger->AnimatedTargets);
            foreach (var targetEntity in animatedTargets)
            {
                if (!frame.Unsafe.TryGetPointer<AnimatedTransform>(targetEntity, out var animatedTransform)) return;

                animatedTransform->IsActive = active;
            }
        }
    }
}
