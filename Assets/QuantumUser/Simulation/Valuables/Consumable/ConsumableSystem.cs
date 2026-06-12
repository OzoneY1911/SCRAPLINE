namespace Quantum
{
    public unsafe class ConsumableSystem : SystemSignalsOnly, ISignalOnValuableUseRequested
    {
        public void OnValuableUseRequested(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
            if (valuable->IsShopValuable) return;
            if (!frame.Unsafe.TryGetPointer<Health>(playerEntity, out var health)) return;
            if (!frame.Unsafe.TryGetPointer<Consumable>(valuableEntity, out var consumable)) return;

            if (consumable->IsUsed) return;

            var config = frame.FindAsset<ValuableConfig>(valuable->Config) as ConsumableConfig;
            if (health->Current == health->Max) return;

            frame.Signals.OnHealthChanged(playerEntity, config.HealthDelta);

            consumable->IsUsed = true;
            frame.Events.OnConsumableUsed(valuableEntity);
        }
    }
}
