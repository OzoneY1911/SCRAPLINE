using TMPro;
using UnityEngine;

namespace Quantum
{
    public unsafe class GameLocationSelectorView : QuantumEntityViewComponent
    {
        [SerializeField] private Material _selectionMaterial;

        [SerializeField] private GameObject _selectDestinationObject;
        [SerializeField] private GameObject _launchTextObject;
        [SerializeField] private Renderer _mapChangerRenderer;

        [SerializeField] private GameObject[] _locationPanelObjects;

        private Renderer _renderer;
        private Material _originalMaterial;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _originalMaterial = _renderer.sharedMaterial;
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

            _selectDestinationObject.SetActive(false);
            _launchTextObject.SetActive(true);
            _mapChangerRenderer.enabled = true;

            for (int i = 0; i < _locationPanelObjects.Length; i++)
            {
                _locationPanelObjects[i].SetActive((int) frame.Global->SelectedLocation.Type == i + 1);
            }
        }
    }
}
