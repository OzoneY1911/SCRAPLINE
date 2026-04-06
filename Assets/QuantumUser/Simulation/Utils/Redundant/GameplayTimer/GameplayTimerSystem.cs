namespace Quantum
{
    public unsafe class GameplayTimerSystem : SystemMainThreadFilter<GameplayTimerSystem.InteractableFilter>
    {
        public struct InteractableFilter
        {
            public EntityRef Entity;
            public Interactable* Interactable;
        }

        public override void Update(Frame frame, ref InteractableFilter filter)
        {
            if (!frame.IsVerified) return;

            filter.Interactable->CooldownTimer.Tick(frame.DeltaTime);
        }
    }
}
