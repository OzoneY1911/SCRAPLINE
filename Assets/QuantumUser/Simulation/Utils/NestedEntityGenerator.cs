using Photon.Deterministic;

namespace Quantum
{
    public unsafe static class NestedEntityGenerator
    {
        public static void GenerateNestedEntities(Frame frame, EntityRef parentEntity)
        {
            if (frame.Unsafe.TryGetPointer<NestedParentEntity>(parentEntity, out var nestedParent))
            {
                var nestedEntities = frame.ResolveList<NestedChildEntity>(nestedParent->NestedEntities);

                foreach (var nestedChild in nestedEntities)
                {
                    var nestedEntity = frame.Create(nestedChild.Prototype);
                    frame.Unsafe.TryGetPointer<Transform3D>(nestedEntity, out var nestedTransform);
                    nestedTransform->Teleport(frame, nestedChild.SpawnPosition);
                    nestedTransform->Teleport(frame, FPQuaternion.Euler(nestedChild.SpawnRotation));
                }
            }
        }
    }
}