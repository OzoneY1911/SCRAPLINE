using Quantum;
using Quantum.Menu;
using System;
using UnityEngine;

public enum UIState
{
    None,
    Chat,
    SessionMenu,
    Customization
}

public class InputManager : PersistentSingletonMono<InputManager>
{
    private PlayerControls _playerControls;

    public PlayerControls PlayerControls => _playerControls;

    private UIState _currentState = UIState.None;

    private QuantumMenuUICustomization _menuCustomization;

    public event Action<UIState> UIStateChanged;

    protected override void Awake()
    {
        base.Awake();
        _playerControls = new PlayerControls();

        _menuCustomization = FindAnyObjectByType<QuantumMenuUICustomization>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        _playerControls.Enable();
        SetUIState(UIState.None);

        QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
        QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);

        _menuCustomization.MenuCustomizationClosed += OnMenuCustomizationClosed;
    }

    private void OnGameStarted(CallbackGameStarted callback)
    {
        _playerControls.Enable();

        if (_currentState != UIState.None) SetUIState(UIState.None);
    }

    private void OnDisable()
    {
        _playerControls.Disable();
        _menuCustomization.MenuCustomizationClosed -= OnMenuCustomizationClosed;
    }

    private void OnGameDestroyed(CallbackGameDestroyed callback)
    {
        _playerControls.Disable();
    }

    private void Update()
    {
        if (_playerControls.PersistentMap.ToggleSessionMenu.WasPressedThisFrame())
            ToggleUIState(UIState.SessionMenu);

        if (_playerControls.PersistentMap.ToggleChat.WasPressedThisFrame())
            ToggleUIState(UIState.Chat);

        if (_playerControls.PersistentMap.ToggleCustomization.WasPressedThisFrame())
            ToggleUIState(UIState.Customization);
    }

    private void ToggleUIState(UIState state)
    {
        SetUIState(_currentState == state ? UIState.None : state);
    }

    public void SetUIState(UIState state)
    {
        if (_currentState != UIState.None && state != UIState.None) return;

        _currentState = state;

        if (state == UIState.None)
        {
            _playerControls.Main.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            _playerControls.Main.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        UIStateChanged?.Invoke(state);
    }

    private void OnMenuCustomizationClosed()
    {
        SetUIState(UIState.None);
    }
}