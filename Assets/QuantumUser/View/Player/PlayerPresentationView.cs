using System.Collections.Generic;
using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerPresentationView : QuantumEntityViewComponent
    {
        [SerializeField] private List<GameObject> _slotObjects;

        private EntityRef[] _slotEntities;
        private GameObject[] _slotVisuals;

        private int _selectedSlotIndex = (int)SlotIndex.None;

        public override void OnActivate(Frame frame)
        {
            if (!frame.Unsafe.TryGetPointer<Player>(EntityRef, out Player* player)) return;
            if (!Game.PlayerIsLocal(player->PlayerRef)) return;

            _slotEntities = new EntityRef[_slotObjects.Count];
            _slotVisuals = new GameObject[_slotObjects.Count];

            QuantumEvent.Subscribe<EventInventorySlotSelected>(this, OnEventInventorySlotSelected);
            QuantumEvent.Subscribe<EventValuableCollected>(this, OnEventValuableCollected);
            QuantumEvent.Subscribe<EventValuableDropped>(this, OnEventValuableDropped);

            QuantumEvent.Subscribe<EventFlashlightToggled>(this, OnEventFlashlightToggled);
        }

        private void OnEventInventorySlotSelected(EventInventorySlotSelected e)
        {
            if (e.Entity != EntityRef) return;

            if (!VerifiedFrame.IsVerified) return;

            _selectedSlotIndex = (int)e.SlotIndex;

            for (int i = 0; i < _slotObjects.Count; i++)
            {
                if (_slotVisuals[i] == null) continue;
                _slotVisuals[i].SetActive(i == (int)e.SlotIndex);
            }
        }

        private void OnEventValuableCollected(EventValuableCollected e)
        {
            if (e.PlayerEntity != EntityRef) return;

            var frame = VerifiedFrame;
            if (!frame.Unsafe.TryGetPointer<Valuable>(e.ValuableEntity, out var valuable)) return;

            var fpsPrefab = frame.FindAsset<ValuableConfig>(valuable->Config).FPSPrefab;
            var index = (int)e.SlotIndex;

            _slotEntities[index] = e.ValuableEntity;
            _slotVisuals[index] = Instantiate(fpsPrefab, _slotObjects[index].transform);

            _slotVisuals[index].SetActive(index == _selectedSlotIndex);

            if (frame.Unsafe.TryGetPointer<Flashlight>(e.ValuableEntity, out var flashlight))
            {
                var light = _slotVisuals[index].GetComponentInChildren<Light>(true);
                light.enabled = flashlight->IsOn;
            }
        }

        private void OnEventValuableDropped(EventValuableDropped e)
        {
            if (e.PlayerEntity != EntityRef) return;
            if (!VerifiedFrame.IsVerified) return;

            int index = (int)e.SlotIndex;

            Destroy(_slotVisuals[index]);

            _slotEntities[index] = EntityRef.None;
            _slotVisuals[index] = null;
        }

        private void OnEventFlashlightToggled(EventFlashlightToggled e)
        {
            if (e.FlashlightEntity != _slotEntities[_selectedSlotIndex]) return;

            _slotVisuals[_selectedSlotIndex].GetComponentInChildren<Light>().enabled = e.IsOn;
        }
    }
}
