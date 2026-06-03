using Quantum;
using Quantum.Collections;

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

    public static bool EntityIsInGroup(Frame frame, EntityRef groupOwner, EntityRef targetEntity)
    {
        if (frame.Has<EntityGroup>(groupOwner))
        {
            foreach (var nestedEntity in frame.GetEntityGroupIterator(groupOwner))
            {
                if (nestedEntity.Item1 == targetEntity)
                {
                    return true;
                }
                else
                {
                    continue;
                }
            }
        }
        return false;
    }

    public static void AddEntityToList(Frame frame, EntityRef entity, QListPtr<EntityRef> listPtr)
    {
        var list = frame.ResolveList(listPtr);
        list.Add(entity);
    }

    public static void ClearEntityList(Frame frame, QListPtr<EntityRef> listPtr)
    {
        var list = frame.ResolveList(listPtr);

        foreach (var entity in list)
        {
            if (frame.Has<EntityGroup>(entity))
            {
                foreach (var nestedEntity in frame.GetEntityGroupIterator(entity))
                {
                    frame.Destroy(nestedEntity.Item1);
                }
            }

            frame.Destroy(entity);
        }
    }

    public static void DisableEntityPhysics(Frame frame, EntityRef entity)
    {
        var valuableBody = frame.Unsafe.GetPointer<PhysicsBody3D>(entity);
        var valuableCollider = frame.Unsafe.GetPointer<PhysicsCollider3D>(entity);
        valuableBody->Enabled = false;
        valuableCollider->Enabled = false;
    }

    public static void RestoryEntityPhysicsAndVelocity(Frame frame, EntityRef entity, PhysicsBody3D* parentBody)
    {
        var valuableBody = frame.Unsafe.GetPointer<PhysicsBody3D>(entity);
        var valuableCollider = frame.Unsafe.GetPointer<PhysicsCollider3D>(entity);

        valuableBody->Velocity = parentBody->Velocity;
        valuableBody->AngularVelocity = parentBody->AngularVelocity;
        valuableBody->Enabled = true;
        valuableCollider->Enabled = true;
    }
}