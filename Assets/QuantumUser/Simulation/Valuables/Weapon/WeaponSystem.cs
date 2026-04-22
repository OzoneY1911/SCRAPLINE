namespace Quantum
{
    public unsafe class WeaponSystem : SystemMainThreadFilter<WeaponSystem.Filter>, ISignalOnValuableUseRequested, ISignalOnComponentAdded<Weapon>, ISignalOnValuableReloadRequested
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Weapon* Weapon;
        }

        public void OnAdded(Frame frame, EntityRef entity, Weapon* weapon)
        {
            var config = frame.FindAsset(weapon->Config);

            weapon->CurrentAmmo = config.MagSize;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var weapon = filter.Weapon;

            if (weapon->ReloadCooldown.HasStoppedThisFrame(frame))
            {
                var config = frame.FindAsset(weapon->Config);

                if (config.WeaponType == WeaponType.Gun)
                {
                    weapon->CurrentAmmo = config.MagSize;
                }
            }
        }

        public void OnValuableUseRequested(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Weapon>(valuableEntity, out Weapon* weapon)) return;
            if (weapon->UseCooldown.IsRunning(frame)) return;
            if (weapon->ReloadCooldown.IsRunning(frame)) return;

            var config = frame.FindAsset(weapon->Config);

            if (config.WeaponType == WeaponType.Gun)
            {
                if (weapon->CurrentAmmo != 0)
                {
                    weapon->CurrentAmmo--;
                }
                else
                {
                    return;
                }
            }

            Use(frame, playerEntity, config);

            weapon->UseCooldown = FrameTimer.FromSeconds(frame, 60 / config.FireRate);
        }

        public void OnValuableReloadRequested(Frame frame, EntityRef playerEntity, EntityRef valuableEntity)
        {
            if (!frame.Unsafe.TryGetPointer<Weapon>(valuableEntity, out Weapon* weapon)) return;
            if (weapon->UseCooldown.IsRunning(frame)) return;

            var config = frame.FindAsset(weapon->Config);

            if (config.WeaponType == WeaponType.Gun)
            {
                if (weapon->CurrentAmmo == config.MagSize)
                {
                    return;
                }
            }

            weapon->ReloadCooldown = FrameTimer.FromSeconds(frame, config.ReloadTime);
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