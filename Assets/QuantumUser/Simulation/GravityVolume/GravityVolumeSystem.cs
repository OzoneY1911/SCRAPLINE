using Photon.Deterministic;

namespace Quantum
{
    public unsafe class GravityVolumeSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D, ISignalOnTriggerExit3D, ISignalOnComponentAdded<GravityVolume>
    {
        public void OnAdded(Frame frame, EntityRef entity, GravityVolume* volume)
        {
            volume->EntityGravityScales = frame.AllocateDictionary<EntityRef, FP>();
        }

        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<GravityVolume>(triggerInfo.Entity, out var volume)) return;
            if (!frame.Unsafe.TryGetPointer<PhysicsBody3D>(triggerInfo.Other, out PhysicsBody3D* body)) return;

            var gravityScales = frame.ResolveDictionary(volume->EntityGravityScales);

            if (!gravityScales.ContainsKey(triggerInfo.Other))
            {
                gravityScales.Add(triggerInfo.Other, body->GravityScale);
            }

            body->GravityScale = FP._0;
        }
         
        public void OnTriggerExit3D(Frame frame, ExitInfo3D triggerInfo)
        {
            if (!frame.Unsafe.TryGetPointer<GravityVolume>(triggerInfo.Entity, out var volume)) return;
            if (!frame.Unsafe.TryGetPointer<PhysicsBody3D>(triggerInfo.Other, out PhysicsBody3D* body)) return;

            var gravityScales = frame.ResolveDictionary(volume->EntityGravityScales);

            if (gravityScales.TryGetValue(triggerInfo.Other, out FP originalGravityScale))
            {
                body->GravityScale = originalGravityScale;
                gravityScales.Remove(triggerInfo.Other);
            }
        }
    }
}
