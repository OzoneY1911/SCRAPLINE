using Quantum;
using Quantum.Menu;
using UnityEngine;

public class SessionMenuController : PersistentSingletonMono<SessionMenuController>
{
    private InputManager _inputManager;
    private GameObject _menuContent;
    private QuantumMenuUISettings _menuUISettings;

    private void OnEnable()
    {
        QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
    }

    private void Start()
    {
        _menuContent = FindAnyObjectByType<QuantumMenuUIGameplay>().transform.GetChild(0).gameObject;
        _menuUISettings = FindAnyObjectByType<QuantumMenuUISettings>(FindObjectsInactive.Include);
    }

    private void OnDisable()
    {
        _inputManager.SessionMenuToggled -= OnSessionMenuToggled;
    }

    private void Update()
    {
        if (_inputManager != null) return;

        _inputManager = FindAnyObjectByType<InputManager>();
        _inputManager.SessionMenuToggled += OnSessionMenuToggled;
    }

    private void OnGameDestroyed(CallbackGameDestroyed callback)
    {
        _menuContent.SetActive(false);
        Destroy(gameObject);
    }

    private void OnSessionMenuToggled()
    {
        _menuContent.SetActive(!_menuContent.activeSelf);

        if (!_menuContent.activeSelf && _menuUISettings.IsShowing)
        {
            _menuUISettings.Hide();
        }
    }
}
