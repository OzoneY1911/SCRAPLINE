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

    private void EnableControls() => _playerControls.Enable();
    private void DisableControls() => _playerControls.Disable();

    public void SetSoloMap(InputActionMap map)
    {
        DisableControls();
        map.Enable();
    }
}
