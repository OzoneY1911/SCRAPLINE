using Quantum;
using Quantum.Menu;
using UnityEngine;

public class SessionMenuController : PersistentSingletonMono<SessionMenuController>
{
    private InputManager _inputManager;

    private GameObject _menuContent;
    private QuantumMenuUISettings _menuSettings;
    private QuantumMenuUICustomization _menuCustomization;
    private ChatUI _chatUI;

    private void OnEnable()
    {
        QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
    }

    private void Start()
    {
        _menuContent = FindAnyObjectByType<QuantumMenuUIGameplay>().transform.GetChild(0).gameObject;
        _menuSettings = FindAnyObjectByType<QuantumMenuUISettings>(FindObjectsInactive.Include);
        _menuCustomization = FindAnyObjectByType<QuantumMenuUICustomization>(FindObjectsInactive.Include);
        _chatUI = FindAnyObjectByType<ChatUI>();
    }

    private void OnDisable()
    {
        if (_inputManager == null) return;
        _inputManager.UIStateChanged -= OnUIStateChanged;
    }

    private void Update()
    {
        if (_inputManager != null) return;

        _inputManager = FindAnyObjectByType<InputManager>();
        _inputManager.UIStateChanged += OnUIStateChanged;
    }

    private void OnGameDestroyed(CallbackGameDestroyed callback)
    {
        _menuContent.SetActive(false);
        Destroy(gameObject);
    }

    private void OnUIStateChanged(UIState state)
    {
        switch (state)
        {
            case UIState.SessionMenu:
                _menuContent.SetActive(true);
                break;
            case UIState.Customization:
                _menuCustomization.Show();
                break;
            case UIState.Chat:
                if (!_chatUI.IsChatFocused)
                {
                    _chatUI.ShowChatUI();
                    _chatUI.StartTyping();
                }
                break;
            case UIState.None:
                _menuContent.SetActive(false);
                if (_menuSettings.IsShowing) _menuSettings.Hide();
                if (_menuCustomization.IsShowing) _menuCustomization.Hide();
                if (_chatUI.IsShowingChat) _chatUI.TrySendMessage();
                break;
        }
    }
}
