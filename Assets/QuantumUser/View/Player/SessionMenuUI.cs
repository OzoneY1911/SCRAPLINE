using UnityEngine;

public class SessionMenuUI : MonoBehaviour
{
    [Header("Global References")]
    [SerializeField] private InputManager _inputManager;

    private Canvas _sessionMenuCanvas;

    private void Awake()
    {
        _inputManager = FindAnyObjectByType<InputManager>();
        _sessionMenuCanvas = GetComponent<Canvas>();
        _sessionMenuCanvas.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_inputManager.PlayerControls.PersistentMap.ToggleSessionMenu.WasPressedThisFrame())
        {
            _sessionMenuCanvas.enabled = !_sessionMenuCanvas.enabled;
        }
    }
}
