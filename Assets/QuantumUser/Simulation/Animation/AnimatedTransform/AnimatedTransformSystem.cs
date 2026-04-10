using Photon.Deterministic;

namespace Quantum
{
    public unsafe class AnimatedTransformSystem
        : SystemMainThreadFilter<AnimatedTransformSystem.Filter>,
          ISignalOnComponentAdded<AnimatedTransform>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public AnimatedTransform* AnimatedTransform;
        }

        public void OnAdded(Frame frame, EntityRef entity, AnimatedTransform* anim)
        {
            Transform3D* transform = frame.Unsafe.GetPointer<Transform3D>(entity);

            if (anim->Parent == EntityRef.None)
            {
                anim->InitialLocalPosition = transform->Position;
                return;
            }

            if (frame.Unsafe.TryGetPointer<Transform3D>(anim->Parent, out Transform3D* parent))
            {
                FPQuaternion invRot = FPQuaternion.Conjugate(parent->Rotation);

                anim->InitialLocalPosition =
                    invRot * (transform->Position - parent->Position);
            }
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            AnimatedTransform* anim = filter.AnimatedTransform;
            Transform3D* transform = filter.Transform;

            FP target = anim->IsActive ? FP._1 : FP._0;

            anim->Progress = FPMath.MoveTowards(
                anim->Progress,
                target,
                anim->Speed * frame.DeltaTime
            );

            FPVector3 localPos = FPVector3.Lerp(
                anim->InitialLocalPosition,
                anim->InitialLocalPosition + anim->TargetOffset,
                anim->Progress
            );

            if (anim->Parent == EntityRef.None)
            {
                transform->Position = localPos;
                return;
            }

            if (frame.Unsafe.TryGetPointer<Transform3D>(anim->Parent, out Transform3D* parent))
            {
                transform->Position =
                    parent->Position +
                    parent->Rotation * localPos;
            }
        }
    }
}