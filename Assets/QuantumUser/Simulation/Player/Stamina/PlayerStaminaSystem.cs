using Photon.Deterministic;
using System;

namespace Quantum
{
    public unsafe class PlayerStaminaSystem : SystemMainThreadFilter<PlayerStaminaSystem.Filter>, ISignalOnComponentAdded<PlayerStamina>, ISignalOnPlayerJump
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public PlayerMovement* Movement;
            public PlayerStamina* Stamina;
        }

        public void OnAdded(Frame frame, EntityRef entity, PlayerStamina* stamina)
        {
            stamina->Current = stamina->Max;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var movement = filter.Movement;
            var stamina = filter.Stamina;

            if (movement->IsRunning)
            {
                stamina->Current = FPMath.Max(stamina->Current - stamina->DrainPerSec * frame.DeltaTime, FP._0);
            }
            else
            {
                stamina->Current = FPMath.Min(stamina->Current + stamina->RegenPerSec * frame.DeltaTime, stamina->Max);
            }

            if (stamina->Current <= FP._0)
            {
                stamina->IsExhausted = true;
            }

            if (stamina->IsExhausted && stamina->Current >= stamina->RecoverThreshold)
            {
                stamina->IsExhausted = false;
            }
        }

        public void OnPlayerJump(Frame frame, EntityRef entity)
        {
            var stamina = frame.Unsafe.GetPointer<PlayerStamina>(entity);

            stamina->Current = FPMath.Max(stamina->Current - stamina->CostPerJump, FP._0);
        }
    }
}