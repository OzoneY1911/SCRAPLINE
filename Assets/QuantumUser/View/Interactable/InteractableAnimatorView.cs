using Quantum;
using UnityEngine;

public unsafe class InteractableAnimatorView : QuantumEntityViewComponent
{
    [SerializeField] private Animator _animator;

    public override void OnActivate(Frame frame)
    {
        QuantumEvent.Subscribe<EventInteractableInteracted>(this, OnEventInteractableInteracted);
    } 

    private void OnEventInteractableInteracted(EventInteractableInteracted e)
    {
        if (e.Entity != EntityRef) return;

        var frame = QuantumRunner.Default.Game.Frames.Predicted;
        if (frame == null) return;

        var interactableAnimator = frame.Unsafe.GetPointer<InteractableAnimator>(EntityRef);

        _animator.SetBool("IsToggled", interactableAnimator->IsToggled);
    }
}
