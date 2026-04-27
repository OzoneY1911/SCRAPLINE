using System;
using UnityEngine;

namespace Quantum.Menu
{
    public partial class QuantumMenuUICustomization : QuantumMenuUIScreen
    {
        [SerializeField] protected UnityEngine.UI.Button _backButton;

        public event Action MenuCustomizationClosed;

        protected virtual void OnBackButtonPressed()
        {
            var uiGameplay = Controller.Get<QuantumMenuUIGameplay>();
            if (uiGameplay.IsShowing)
            {
                Hide();
                if (!uiGameplay.transform.GetChild(0).gameObject.activeSelf)
                {
                    MenuCustomizationClosed?.Invoke();
                }
            }
            else
            {
                Controller.Show<QuantumMenuUIMain>();
            }
        }
    }
}