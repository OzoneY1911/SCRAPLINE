using Quantum;

public unsafe static class EntityUtils
{
    public static void SyncEntityGroupTransform(Frame frame, EntityRef groupOwner, MapPointData groupPoint)
    {
        if (frame.Has<EntityGroup>(groupOwner))
        {
            foreach (var nestedEntity in frame.GetEntityGroupIterator(groupOwner))
            {
                var nestedTransform = frame.Unsafe.GetPointer<Transform3D>(nestedEntity.Item1);

                var localPosition = nestedTransform->Position;
                var localRotation = nestedTransform->Rotation;

                nestedTransform->Position = groupPoint.Position + (groupPoint.Rotation * localPosition);
                nestedTransform->Rotation = groupPoint.Rotation * localRotation;
            }
        }
    }
}