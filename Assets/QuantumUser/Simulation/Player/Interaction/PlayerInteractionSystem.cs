using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class PlayerInteractionSystem : SystemMainThreadFilter<PlayerInteractionSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public Player* Player;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Player;
            var input = frame.GetPlayerInput(filter.Player->PlayerRef);

            if (input->Interact.WasPressed)
            {
                var hit = PlayerPhysicsUtils.PlayerInteractionHitscan(frame, player);

                if (hit.HasValue)
                {
                    var hitEntity = hit.Value.Entity;
                    if (frame.Has<Interactable>(hitEntity))
                    {
                        frame.Signals.OnInteract(frame.Unsafe.GetPointer<Interactable>(hitEntity));
                    }
                }
            }
        }
    }
}
