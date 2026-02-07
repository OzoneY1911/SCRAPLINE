using System.Collections.Generic;
using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerPresentationView : QuantumEntityViewComponent
    {
        [SerializeField] private List<GameObject> _inventorySlotObjects;

        public override void OnActivate(Frame frame)
        {
            if (!frame.Unsafe.TryGetPointer<Player>(EntityRef, out Player* player)) return;
            if (!Game.PlayerIsLocal(player->PlayerRef)) return;

            QuantumEvent.Subscribe<EventInventorySlotSelected>(this, OnEventInventorySlotSelected);
            QuantumEvent.Subscribe<EventValuableCollected>(this, OnEventValuableCollected);
            QuantumEvent.Subscribe<EventValuableDropped>(this, OnEventValuableDropped);
        }

        private void OnEventInventorySlotSelected(EventInventorySlotSelected e)
        {
            if (e.Entity != EntityRef) return;

            if (!VerifiedFrame.IsVerified) return;

            for (int i = 0; i < _inventorySlotObjects.Count; i++)
            {
                if (i != (int)e.SlotIndex)
                {
                    _inventorySlotObjects[i].transform.GetChild(0).gameObject.SetActive(false);
                }
                else
                {
                    _inventorySlotObjects[i].transform.GetChild(0).gameObject.SetActive(true);
                }
            }
        }

        private void OnEventValuableCollected(EventValuableCollected e)
        {
            if (e.PlayerEntity != EntityRef) return;

            Frame frame = VerifiedFrame;
            if (!frame.Unsafe.TryGetPointer<Valuable>(e.ValuableEntity, out var valuable)) return;

            var fpsPrefab = VerifiedFrame.FindAsset<ValuableConfig>(valuable->Config).FPSPrefab;

            Instantiate(fpsPrefab, _inventorySlotObjects[(int)e.SlotIndex].transform);
            _inventorySlotObjects[(int)e.SlotIndex].transform.GetChild(0).gameObject.SetActive(false);
        }

        private void OnEventValuableDropped(EventValuableDropped e)
        {
            if (e.PlayerEntity != EntityRef) return;
            
            if (!VerifiedFrame.IsVerified) return;
            
            Destroy(_inventorySlotObjects[(int)e.SlotIndex].transform.GetChild(0).gameObject);
        }
    }
}
