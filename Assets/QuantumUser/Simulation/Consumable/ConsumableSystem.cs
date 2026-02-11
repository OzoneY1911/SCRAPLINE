namespace Quantum
{
    public unsafe class ConsumableSystem : SystemSignalsOnly, ISignalOnValuableUseRequested
    {
        public void OnValuableUseRequested(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;
            if (!frame.Unsafe.TryGetPointer<Health>(playerEntity, out var health)) return;

            var config = frame.FindAsset<ValuableConfig>(valuable->Config) as ConsumableConfig;
            if (health->Current == health->Max) return;

            frame.Signals.OnHealthChanged(playerEntity, config.HealthDelta);
            frame.Signals.OnValuableDropRequested(playerEntity, valuableEntity);
        }
    }
}
