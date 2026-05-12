using Photon.Deterministic;

namespace Quantum
{
    public unsafe class GravityVolumeSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D
    {
        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D triggerInfo)
        {
            if (!frame.Has<GravityVolume>(triggerInfo.Entity)) return;
            if (!frame.Unsafe.TryGetPointer<PhysicsBody3D>(triggerInfo.Other, out PhysicsBody3D* body)) return;

            body->GravityScale = FP._0;
        }

        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {
            if (!frame.Has<GravityVolume>(triggerInfo.Entity)) return;
            if (!frame.Unsafe.TryGetPointer<PhysicsBody3D>(triggerInfo.Other, out PhysicsBody3D* body)) return;

            body->GravityScale = 1;
        }
    }
}
