using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerControls _playerControls;

    public PlayerControls PlayerControls => _playerControls;

    private InputActionMap _previousMap;
    private InputActionMap _currentMap;

    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        EnableControls();
        SetSoloMap(_playerControls.Main);
    }

    private void OnDisable()
    {
        DisableControls();
    }

    private void Update()
    {
        if (_playerControls.PersistentMap.Pause.WasPressedThisFrame())
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

        if (_playerControls.PersistentMap.ToggleChat.WasPressedThisFrame())
        {
            if (_playerControls.PersistentMap.Pause.enabled)
            {
                SetSoloMap(_playerControls.PersistentMap);
                _playerControls.PersistentMap.Pause.Disable();
            }
            else
            {
                EnablePreviousMap();
                _playerControls.PersistentMap.Pause.Enable();
            }
        }
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
}
