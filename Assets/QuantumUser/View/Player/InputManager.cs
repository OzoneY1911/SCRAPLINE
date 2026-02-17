using Quantum;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : PersistentSingletonMono<InputManager>
{
    private PlayerControls _playerControls;

    public PlayerControls PlayerControls => _playerControls;

    private InputActionMap _previousMap;
    private InputActionMap _currentMap;

    protected override void Awake()
    {
        base.Awake();

        _playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        EnableControls();
        SetSoloMap(_playerControls.Main);

        QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameStarted);
        QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
    }

    private void OnGameStarted(CallbackGameStarted callback)
    {
        EnableControls();
        SetSoloMap(_playerControls.Main);
    }

    private void OnGameDestroyed(CallbackGameDestroyed callback)
    {
         DisableControls();
    }

    private void OnDisable()
    {
        DisableControls();
    }

    private void Update()
    {
        HandleSessionMenu();
        HandleChat();
    }

    private void EnableControls() => _playerControls.Enable();
    private void DisableControls() => _playerControls.Disable();

    private void ToggleCursor()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void SetSoloMap(InputActionMap map)
    {
        if (_currentMap != null)
        {
            _previousMap = _currentMap;
        }
        _currentMap = map;

        DisableControls();
        _playerControls.PersistentMap.Enable();
        map.Enable();
    }

    public void EnablePreviousMap()
    {
        _previousMap?.Enable();
        _currentMap = _previousMap;
        _previousMap = null;
    }

    private void HandleSessionMenu()
    {
        if (_playerControls.PersistentMap.ToggleSessionMenu.WasPressedThisFrame())
        {
            ToggleCursor();

            if (_playerControls.PersistentMap.ToggleChat.enabled)
            {
                SetSoloMap(_playerControls.PersistentMap);
                _playerControls.PersistentMap.ToggleChat.Disable();
            }
            else
            {
                EnablePreviousMap();
                _playerControls.PersistentMap.ToggleChat.Enable();
            }
        }
    }

    private void HandleChat()
    {
        if (_playerControls.PersistentMap.ToggleChat.WasPressedThisFrame())
        {
            if (_playerControls.PersistentMap.ToggleSessionMenu.enabled)
            {
                SetSoloMap(_playerControls.PersistentMap);
                _playerControls.PersistentMap.ToggleSessionMenu.Disable();
            }
            else
            {
                EnablePreviousMap();
                _playerControls.PersistentMap.ToggleSessionMenu.Enable();
            }
        }
    }
}
