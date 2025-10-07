using Photon.Deterministic;

namespace Quantum
{
    public unsafe class TeleporterSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D
    {
        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<Transform3D>(triggerInfo.Other, out var otherTransform)) return;

            if (!frame.Unsafe.TryGetPointer<Teleporter>(triggerInfo.Entity, out var teleporter)) return;

            if (!frame.Unsafe.TryGetPointer<Transform3D>(teleporter->ExitEntity, out var exitTransform)) return;

            if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(triggerInfo.Other, out var otherBody))
            {
                otherBody->Velocity = FPVector3.Zero;
            }

            otherTransform->Teleport(frame, exitTransform->Position);
        }
    }
}
