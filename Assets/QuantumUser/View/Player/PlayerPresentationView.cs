using System;
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
            QuantumEvent.Subscribe<EventOnConsumableUsed>(this, OnEventConsumableUsed);

            if (!VerifiedFrame.Unsafe.TryGetPointer<PlayerInventory>(EntityRef, out var playerInventory)) return;

            SelectSlotValuable(EntityRef, (int)SlotIndex.None);
            for (int i = 0; i < playerInventory->Slots.Length; i++)
            {
                var valuableEntity = playerInventory->Slots[i];
                if (valuableEntity == EntityRef.None) continue;

                CollectSlotValuable(EntityRef, valuableEntity, i);
            }
            SelectSlotValuable(EntityRef, (int)playerInventory->SelectedSlotIndex);
        }

        private void OnEventInventorySlotSelected(EventInventorySlotSelected e)
        {
            SelectSlotValuable(e.PlayerEntity, (int)e.SlotIndex);
        }

        private void OnEventValuableCollected(EventValuableCollected e)
        {
            CollectSlotValuable(e.PlayerEntity, e.ValuableEntity, (int)e.SlotIndex);
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

        private void OnEventConsumableUsed(EventOnConsumableUsed e)
        {
            if (e.ConsumableEntity != _slotEntities[_selectedSlotIndex]) return;

            var animator = _slotVisuals[_selectedSlotIndex].GetComponentInChildren<Animator>();
            animator.SetBool("IsUsed", true);
        }

        private void SelectSlotValuable(EntityRef playerEntity, int slotIndex)
        {
            if (playerEntity != EntityRef) return;

            if (!VerifiedFrame.IsVerified) return;

            _selectedSlotIndex = (int)slotIndex;

            for (int i = 0; i < _slotObjects.Count; i++)
            {
                if (_slotVisuals[i] == null) continue;
                _slotVisuals[i].SetActive(i == (int)slotIndex);
            }
        }

        private void CollectSlotValuable(EntityRef playerEntity, EntityRef valuableEntity, int slotIndex)
        {
            if (playerEntity != EntityRef) return;

            if (!VerifiedFrame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;

            var fpsPrefab = VerifiedFrame.FindAsset<ValuableConfig>(valuable->Config).FPSPrefab;
            var index = (int)slotIndex;

            _slotEntities[index] = valuableEntity;
            _slotVisuals[index] = Instantiate(fpsPrefab, _slotObjects[index].transform);

            if (VerifiedFrame.Unsafe.TryGetPointer<Flashlight>(valuableEntity, out var flashlight))
            {
                var light = _slotVisuals[index].GetComponentInChildren<Light>(true);
                light.enabled = flashlight->IsOn;
            }
            else if (VerifiedFrame.Unsafe.TryGetPointer<Consumable>(valuableEntity, out var consumable))
            {
                if (consumable->IsUsed)
                {
                    var animator = _slotVisuals[index].GetComponentInChildren<Animator>();
                    animator.SetBool("IsUsed", true);
                    animator.keepAnimatorStateOnDisable = true;
                }
            }

            _slotVisuals[index].SetActive(index == _selectedSlotIndex);
        }
    }
}
