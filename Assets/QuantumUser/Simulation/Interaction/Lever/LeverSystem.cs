using Photon.Deterministic;

namespace Quantum
{
    public unsafe class LeverSystem : SystemMainThreadFilter<LeverSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public Lever* Lever;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var lever = filter.Lever;
            var transform = filter.Transform;

            if (!lever->IsInitialized)
            {
                lever->InitialRotation = filter.Transform->Rotation;
                lever->InitialAngle = filter.Transform->Rotation.AsEuler.X;
                lever->CurrentAngle = lever->InitialAngle;
                lever->IsInitialized = true;
            }
            
            if (!lever->IsActivated)
            {
                FP epsilon = FP._0_20;

                if (FPMath.Abs(lever->CurrentAngle - lever->MaxAngle) <= epsilon)
                {
                    lever->IsActivated = true;
                    UnityEngine.Debug.Log("Lever Activated");
                }
            }
            else
            {
                FP epsilon = FP._0_20;

                if (FPMath.Abs(lever->CurrentAngle - lever->InitialAngle) <= epsilon)
                {
                    lever->IsActivated = false;
                    UnityEngine.Debug.Log("Lever Deactivated");
                }
            }

            if (!lever->IsBeingInteracted)
            {
                var deltaAngle = lever->CurrentAngle - lever->InitialAngle;

                var targetAngle = (deltaAngle > lever->MaxAngle - lever->CurrentAngle)
                    ? lever->MaxAngle
                    : lever->InitialAngle;

                lever->CurrentAngle = FPMath.Lerp(
                    lever->CurrentAngle,
                    targetAngle,
                    lever->AngleResetSpeed * frame.DeltaTime);

                filter.Transform->Rotation = filter.Lever->InitialRotation * FPQuaternion.Euler(new FPVector3(deltaAngle, 0, 0));
            }
        }
    }
}
