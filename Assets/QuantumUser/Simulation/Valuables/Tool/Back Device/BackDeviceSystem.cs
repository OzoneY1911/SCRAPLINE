namespace Quantum
{
    public unsafe class BackDeviceSystem : SystemMainThreadFilter<BackDeviceSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public BackDevice* Device;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Device->CurrentOwner != EntityRef.None)
            {
                var player = frame.Unsafe.GetPointer<Player>(filter.Device->CurrentOwner);
            }
        }
    }
}
