using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerControls _playerControls;

    public PlayerControls PlayerControls => _playerControls;

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
        DisableControls();
        _playerControls.PersistentMap.Enable();
        map.Enable();
    }
}
