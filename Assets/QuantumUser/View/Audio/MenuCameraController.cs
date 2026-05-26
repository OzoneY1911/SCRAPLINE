using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCameraController : MonoBehaviour
{
    private StudioListener _listener;

    private void Awake()
    {
        _listener = GetComponent<StudioListener>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "HubScene")
        {
            _listener.enabled = false;
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == "HubScene")
        {
            _listener.enabled = true;
        }
    }
}