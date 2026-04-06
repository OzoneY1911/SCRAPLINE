namespace Quantum
{
    public unsafe class PlayerUseSystem : SystemMainThreadFilter<PlayerUseSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public PlayerInventory* PlayerInventory;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

            if (!PlayerInventoryUtils.IsSlotSelected(filter.PlayerInventory)) return;
            if (PlayerInventoryUtils.IsSelectedSlotEmpty(filter.PlayerInventory)) return;

            var valuableEntity = filter.PlayerInventory->Slots[(int)filter.PlayerInventory->SelectedSlotIndex];

            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;

            var config = frame.FindAsset(valuable->Config);

            bool shouldUse = false;

            switch (config.UseMode)
            {
                case UseMode.Press:
                    shouldUse = input->Use.WasPressed;
                    break;

                case UseMode.Hold:
                    shouldUse = input->Use.IsDown;
                    break;
            }

            if (!shouldUse) return;

            frame.Signals.OnValuableUseRequested(filter.Entity, valuableEntity);
        }
    }
}
