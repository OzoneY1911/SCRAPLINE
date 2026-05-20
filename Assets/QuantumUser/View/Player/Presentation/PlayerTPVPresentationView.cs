using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerTPVPresentationView : QuantumEntityViewComponent
    {
        [SerializeField] private Transform _handSocket;
        [SerializeField] private Transform _backDeviceSocket;

        private GameObject _backDeviceVisual;

        private EntityRef _currentEntity;
        private GameObject _currentVisual;

        public override void OnActivate(Frame frame)
        {
            QuantumEvent.Subscribe<EventBackDeviceCollected>(this, OnEventBackDeviceCollected);
            QuantumEvent.Subscribe<EventBackDeviceDropped>(this, OnEventBackDeviceDropped);
        }

        public override void OnUpdateView()
        {
            if (!VerifiedFrame.Unsafe.TryGetPointer<Player>(EntityRef, out Player* player)) return;

            if (!VerifiedFrame.Unsafe.TryGetPointer<PlayerInventory>(EntityRef, out var inventory)) return;

            EntityRef selected = EntityRef.None;

            if (inventory->SelectedSlotIndex != SlotIndex.None)
            {
                selected = inventory->Slots[(int)inventory->SelectedSlotIndex];
            }

            if (selected != _currentEntity)
            {
                if (_currentVisual != null)
                {
                    Destroy(_currentVisual);
                    _currentVisual = null;
                }

                _currentEntity = selected;

                if (selected != EntityRef.None)
                {
                    if (VerifiedFrame.Unsafe.TryGetPointer<Valuable>(selected, out var valuable))
                    {
                        var prefab = VerifiedFrame
                            .FindAsset<ValuableConfig>(valuable->Config)
                            .TPVPrefab;

                        _currentVisual = Instantiate(prefab, _handSocket);

                        if (Game.PlayerIsLocal(player->PlayerRef))
                        {
                            var currentVisualRenderers = _currentVisual.GetComponentsInChildren<Renderer>();
                            foreach (var renderer in currentVisualRenderers)
                            {
                                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                            }
                        }
                    }
                }
            }

            if (Game.PlayerIsLocal(player->PlayerRef)) return;

            if (_currentVisual != null && _currentEntity != EntityRef.None)
            {
                InitializeSlotVisual(_currentEntity, _currentVisual);
            }
        }

        private void InitializeSlotVisual(EntityRef entity, GameObject visual)
        {
            if (VerifiedFrame.Unsafe.TryGetPointer<Flashlight>(entity, out var flashlight))
            {
                var flashlightConfig = VerifiedFrame.FindAsset<FlashlightConfig>(flashlight->Config);
                var light = visual.GetComponentInChildren<Light>(true);
                var flashlightRenderer = visual.GetComponentInChildren<Renderer>(true);
                var flashlightMaterials = flashlightRenderer.sharedMaterials;

                light.enabled = flashlight->IsOn;

                flashlightMaterials[1] = flashlight->IsOn
                ? flashlightConfig.IsOnMaterial
                : flashlightConfig.IsOffMaterial;

                flashlightRenderer.sharedMaterials = flashlightMaterials;
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

        private void OnEventBackDeviceCollected(EventBackDeviceCollected e)
        {
            if (EntityRef != e.PlayerEntity) return;

            if (!VerifiedFrame.Unsafe.TryGetPointer<Valuable>(e.BackDeviceEntity, out var valuable)) return;
            var prefab = VerifiedFrame.FindAsset<ValuableConfig>(valuable->Config).TPVPrefab;

            _backDeviceVisual = Instantiate(prefab, _backDeviceSocket);
        }

        private void OnEventBackDeviceDropped(EventBackDeviceDropped e)
        {
            if (EntityRef != e.PlayerEntity) return;

            Destroy(_backDeviceVisual);
            _backDeviceVisual = null;
        }
    }
}