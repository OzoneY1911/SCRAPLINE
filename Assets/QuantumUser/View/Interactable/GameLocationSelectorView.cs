using TMPro;
using UnityEngine;

namespace Quantum
{
    public unsafe class GameLocationSelectorView : QuantumEntityViewComponent
    {
        [SerializeField] private TextMeshProUGUI _selectionTMP;
        [SerializeField] private Material _selectionMaterial;

        private Renderer _renderer;
        private Material _originalMaterial;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _originalMaterial = _renderer.sharedMaterial;
            _selectionTMP.text = "";
        }

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventGameLocationSelected>(this, OnEventGameLocationSelected);
        }

        private void OnEventGameLocationSelected(EventGameLocationSelected e)
        {
            if (e.Entity != EntityRef)
            {
                _renderer.sharedMaterial = _originalMaterial;
                return;
            }

            var frame = QuantumRunner.Default.Game.Frames.Verified;
            if (frame == null) return;

            _renderer.sharedMaterial = _selectionMaterial;
            _selectionTMP.text = $"Selected Location: {frame.Global->SelectedLocation.Type.ToString()}";
        }
    }
}
