using UnityEngine;

public class SessionMenuUI : MonoBehaviour
{
    [Header("Global References")]
    [SerializeField] private InputManager _inputManager;

    private Canvas _sessionMenuCanvas;

    void Awake()
    {
        _sessionMenuCanvas = GetComponent<Canvas>();
        _sessionMenuCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_inputManager.PlayerControls.PersistentMap.ToggleSessionMenu.WasPressedThisFrame())
        {
            _sessionMenuCanvas.enabled = !_sessionMenuCanvas.enabled;
        }
    }
}
