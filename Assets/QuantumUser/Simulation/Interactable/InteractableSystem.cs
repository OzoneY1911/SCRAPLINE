using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class InteractableSystem : SystemSignalsOnly, ISignalOnInteract
    {
        public void OnInteract(Frame frame, Interactable* interactable)
        {
            switch (interactable->Type)
            {
                case InteractableType.ShipStart:
                    if (frame.IsVerified)
                    {
                        frame.Map = frame.FindAsset(interactable->TargetMap);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}