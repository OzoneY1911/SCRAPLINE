using Photon.Deterministic;
using UnityEngine.Scripting;

namespace Quantum
{
    [Preserve]
    public unsafe class InteractableSystem : SystemSignalsOnly, ISignalOnInteract, ISignalOnComponentAdded<Interactable>
    {
        public void OnAdded(Frame frame, EntityRef entity, Interactable* interactable)
        {
            interactable->Entity = entity;
        }

        public void OnInteract(Frame frame, Interactable* interactable)
        {
            if (!frame.IsVerified) return;

            switch (interactable->Type)
            {
                case InteractableType.MapChanger:
                    ProceduralGenerator.GenerateMap(frame, interactable, out var generatedMap);
                                        
                    frame.AddAsset(generatedMap);
                    frame.Map = generatedMap;
                    break;
                case InteractableType.QuotaZoneInteractor:
                    var interactableQuotaZone = frame.Unsafe.GetPointer<InteractableQuotaZone>(interactable->Entity);
                    var quotaZone = frame.Unsafe.GetPointer<QuotaZone>(interactableQuotaZone->TargetQuotaZone);

                    if (!quotaZone->IsActivated)
                    {
                        frame.Signals.OnActivateQuotaZone(interactableQuotaZone->TargetQuotaZone);
                    }
                    else
                    {
                        if (quotaZone->IsSatisfied)
                        {
                            frame.Signals.OnCompleteQuotaZone(interactableQuotaZone->TargetQuotaZone);
                        }
                    }
                    break;
                case InteractableType.InteractableAnimator:
                    var interactableAnimator = frame.Unsafe.GetPointer<InteractableAnimator>(interactable->Entity);

                    if (interactable->CooldownTimer.IsRunning)
                    {
                        return;
                    }
                    else
                    {
                        interactable->CooldownTimer.Start();
                    }

                    interactableAnimator->IsToggled = !interactableAnimator->IsToggled;
                    frame.Events.InteractableInteracted(interactable->Entity);
                    break;
                case InteractableType.GameLocationSelector:
                    var locationSelector = frame.Unsafe.GetPointer<InteractableGameLocationSelector>(interactable->Entity);

                    frame.Global->SelectedLocation = locationSelector->TargetLocation;
                    frame.Events.GameLocationSelected(interactable->Entity);
                    break;
            }
        }
    }
}