namespace Quantum
{
    public unsafe class WeaponSystem : SystemSignalsOnly, ISignalOnValuableUseRequested
    {
        public void OnValuableUseRequested(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Weapon>(valuableEntity, out Weapon* weapon)) return;
            if (weapon->UseCooldown.IsRunning(frame)) return;

            var config = frame.FindAsset(weapon->Config);

            Use(frame, playerEntity, config);

            weapon->UseCooldown = FrameTimer.FromSeconds(frame, 60 / config.FireRate);
        }

        private void Use(Frame frame, EntityRef playerEntity, WeaponConfig config)
        {
            if (!frame.Unsafe.TryGetPointer<Player>(playerEntity, out var player)) return;

            var hit = PlayerPhysicsUtils.PlayerHitscan(frame, player, config.Range);

            if (hit.HasValue)
            {
                var hitEntity = hit.Value.Entity;

                if (frame.Unsafe.TryGetPointer<Health>(hitEntity, out Health* health))
                {
                    frame.Signals.OnHealthChanged(hitEntity, -config.Damage);
                }
            }
        }
    }
}