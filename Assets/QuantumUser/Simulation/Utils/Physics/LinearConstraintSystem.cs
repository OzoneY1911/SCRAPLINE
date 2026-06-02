using Photon.Deterministic;

namespace Quantum
{
    public unsafe class LinearConstraintSystem : SystemMainThreadFilter<LinearConstraintSystem.Filter>, ISignalOnComponentAdded<LinearConstraint>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public PhysicsBody3D* Body;
            public LinearConstraint* Constraint;
        }

        public void OnAdded(Frame frame, EntityRef entity, LinearConstraint* constraint)
        {
            if (!frame.Unsafe.TryGetPointer<Transform3D>(entity, out var transform)) return;

            constraint->AnchorPosition = transform->Position;

            constraint->Axis = transform->Rotation * constraint->LocalAxis;
            constraint->Axis = constraint->Axis.Normalized;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var transform = filter.Transform;
            var body = filter.Body;
            var constraint = filter.Constraint;

            FPVector3 delta = transform->Position - constraint->AnchorPosition;

            FP offset = FPVector3.Dot(delta, constraint->Axis);
            offset = FPMath.Clamp(offset, constraint->MinOffset, constraint->MaxOffset);

            transform->Position = constraint->AnchorPosition + constraint->Axis * offset;
        }
    }
}
