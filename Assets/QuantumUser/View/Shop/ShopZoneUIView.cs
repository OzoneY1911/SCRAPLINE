using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Quantum
{
    public unsafe class ShopZoneUIView : QuantumEntityViewComponent
    {
        [Header("Shop Zone Screen Toggles")]
        [SerializeField] private Toggle _welcomeToggle;
        [SerializeField] private Toggle _priceToggle;
        [SerializeField] private Toggle _noFundsToggle;
        [SerializeField] private Toggle _successToggle;

        [Header("Shop Zone Value Text")]
        [SerializeField] private TextMeshProUGUI _shopZoneValueTMP;

        [Header("Temporary Screen Active Time")]
        [SerializeField] private float _noFundsScreenTime = 3f;
        [SerializeField] private float _successScreenTime = 5f;

        [Header("Shop Zone Material Coloring")]
        [SerializeField] private Material _shopZoneMaterial;
        [SerializeField] private Color _noFundsColor;
        [SerializeField] private Color _successColor;
        [SerializeField] private float _emissionIntensity = 15f;

        private Color _originalColor;

        private Coroutine _temporaryScreenCoroutine;
        private bool _temporaryScreenIsActive;

        public override void OnActivate(Frame frame)
        {
            _originalColor = _shopZoneMaterial.color;

            QuantumEvent.Subscribe<EventShopZoneUpdated>(this, OnEventShopZoneUpdated);
            QuantumEvent.Subscribe<EventShopPurchaseFailed>(this, OnEventShopPurchaseFailed);
            QuantumEvent.Subscribe<EventShopPurchaseSucceeded>(this, OnEventShopPurchaseSucceeded);
        }

        private void OnEventShopZoneUpdated(EventShopZoneUpdated e)
        {
            if (e.Entity != EntityRef) return;
            if (_temporaryScreenIsActive) return;
            UpdateShopScreen();
        }

        private void OnEventShopPurchaseFailed(EventShopPurchaseFailed e)
        {
            if (e.Entity != EntityRef) return;
            if (_temporaryScreenCoroutine != null) StopCoroutine(_temporaryScreenCoroutine);

            SetShopZoneColor(_noFundsColor);
            _temporaryScreenCoroutine = StartCoroutine(SetTemporaryScreen(_noFundsToggle, _noFundsScreenTime));
        }

        private void OnEventShopPurchaseSucceeded(EventShopPurchaseSucceeded e)
        {
            if (e.Entity != EntityRef) return;
            if (_temporaryScreenCoroutine != null) StopCoroutine(_temporaryScreenCoroutine);

            SetShopZoneColor(_successColor);
            _temporaryScreenCoroutine = StartCoroutine(SetTemporaryScreen(_successToggle, _successScreenTime));
        }

        private void UpdateShopScreen()
        {
            var game = QuantumRunner.Default.Game;
            if (game == null) return;
            var frame = game.Frames.Predicted;
            if (frame == null) return;

            if (frame.Unsafe.TryGetPointer<ShopZone>(EntityRef, out var shopZone))
            {
                _shopZoneValueTMP.text = shopZone->InZoneValue.ToString();

                if (frame.ResolveHashSet(shopZone->InZoneValuables).Count == 0)
                {
                    _welcomeToggle.isOn = true;
                }
                else
                {
                    _priceToggle.isOn = true;
                }
            }
        }

        private IEnumerator SetTemporaryScreen(Toggle toggle, float seconds)
        {
            toggle.isOn = true;

            _temporaryScreenIsActive = true;
            yield return new WaitForSeconds(seconds);
            _temporaryScreenIsActive = false;

            SetShopZoneColor(_originalColor);
            UpdateShopScreen();
        }

        private void SetShopZoneColor(Color color)
        {
            _shopZoneMaterial.SetColor("_EmissiveColor", color * _emissionIntensity);
        }
    }
}
