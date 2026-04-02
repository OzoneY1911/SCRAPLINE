using System.Collections.Generic;
using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerFPVPresentationView : QuantumEntityViewComponent
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

            QuantumEvent.Subscribe<EventFlashlightToggled>(this, OnEventFlashlightToggled);
            QuantumEvent.Subscribe<EventOnConsumableUsed>(this, OnEventConsumableUsed);

            if (!VerifiedFrame.Unsafe.TryGetPointer<PlayerInventory>(EntityRef, out var playerInventory)) return;

            for (int i = 0; i < playerInventory->Slots.Length; i++)
            {
                var valuableEntity = playerInventory->Slots[i];
                if (valuableEntity == EntityRef.None) continue;

                _slotEntities[i] = valuableEntity;
                SpawnSlotVisual(valuableEntity, i);

                _selectedSlotIndex = (int)playerInventory->SelectedSlotIndex;
            }
        }

        public override void OnUpdateView()
        {
            if (!VerifiedFrame.Unsafe.TryGetPointer<Player>(EntityRef, out Player* player)) return;
            if (!Game.PlayerIsLocal(player->PlayerRef)) return;

            if (!VerifiedFrame.Unsafe.TryGetPointer<PlayerInventory>(EntityRef, out var playerInventory)) return;

            for (int i = 0; i < playerInventory->Slots.Length; i++)
            {
                var valuableEntity = playerInventory->Slots[i];

                if (valuableEntity != _slotEntities[i])
                {
                    if (_slotVisuals[i] != null)
                    {
                        Destroy(_slotVisuals[i]);
                        _slotVisuals[i] = null;
                    }

                    _slotEntities[i] = valuableEntity;

                    if (valuableEntity != EntityRef.None)
                    {
                        SpawnSlotVisual(valuableEntity, i);
                    }
                }
            }

            _selectedSlotIndex = (int)playerInventory->SelectedSlotIndex;
            if (_selectedSlotIndex == (int)SlotIndex.None || PlayerInventoryUtils.IsSelectedSlotEmpty(playerInventory)) return;

            for (int i = 0; i < _slotVisuals.Length; i++)
            {
                if (_slotVisuals[i] == null) continue;

                bool wasActive = _slotVisuals[i].activeSelf;
                bool isActive = i == _selectedSlotIndex && playerInventory->Slots[i] != EntityRef.None;

                _slotVisuals[i].SetActive(isActive);

                if (!wasActive && isActive)
                {
                    InitializeSlotVisual(_slotEntities[i], _slotVisuals[i]);
                }
            }
        }

        private void SpawnSlotVisual(EntityRef valuableEntity, int slotIndex)
        {
            if (!VerifiedFrame.Unsafe.TryGetPointer<Valuable>(valuableEntity, out var valuable)) return;

            var prefab = VerifiedFrame.FindAsset<ValuableConfig>(valuable->Config).FPVPrefab;

            GameObject visual = Instantiate(prefab, _slotObjects[slotIndex].transform);

            InitializeSlotVisual(valuableEntity, visual);

            _slotVisuals[slotIndex] = visual;
        }

        private void InitializeSlotVisual(EntityRef entity, GameObject visual)
        {
            var visualRenderers = visual.GetComponentsInChildren<Renderer>();
            foreach (var renderer in visualRenderers)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            if (VerifiedFrame.Unsafe.TryGetPointer<Flashlight>(entity, out var flashlight))
            {
                var light = visual.GetComponentInChildren<Light>(true);
                light.enabled = flashlight->IsOn;
            }
            else if (VerifiedFrame.Unsafe.TryGetPointer<Consumable>(entity, out var consumable))
            {
                if (consumable->IsUsed)
                {
                    Animator animator = visual.GetComponentInChildren<Animator>();
                    if (animator != null)
                    {
                        animator.SetBool("IsUsed", true);
                        animator.keepAnimatorStateOnDisable = true;
                    }
                }
            }
        }

        private void OnEventFlashlightToggled(EventFlashlightToggled e)
        {
            for (int i = 0; i < _slotEntities.Length; i++)
            {
                if (e.FlashlightEntity != _slotEntities[i]) continue;

                if (_slotVisuals[i] == null) return;

                var light = _slotVisuals[i].GetComponentInChildren<Light>(true);
                if (light != null)
                {
                    light.enabled = e.IsOn;
                }

                return;
            }
        }

        private void OnEventConsumableUsed(EventOnConsumableUsed e)
        {
            if (e.ConsumableEntity != _slotEntities[_selectedSlotIndex]) return;

            var animator = _slotVisuals[_selectedSlotIndex].GetComponentInChildren<Animator>();
            animator.SetBool("IsUsed", true);
        }
    }
}
