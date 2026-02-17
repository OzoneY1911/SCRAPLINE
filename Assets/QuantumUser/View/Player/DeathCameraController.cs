using Quantum;
using Unity.Cinemachine;
using UnityEngine;

public unsafe class DeathCameraController : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;

    private CinemachineCamera _deathCamera;

    private int _currentPlayerIndex;

    private void Awake()
    {
        _deathCamera = GetComponent<CinemachineCamera>();
        _inputManager = FindAnyObjectByType<InputManager>();
    }

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventEntityDeath>(this, OnEventEntityDeath);
    }

    private void OnEventEntityDeath(EventEntityDeath e)
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (frame == null) return;

        if (frame.Has<Player>(e.Entity))
        {
            if (!QuantumRunner.Default.Game.PlayerIsLocal(frame.Unsafe.GetPointer<Player>(e.Entity)->PlayerRef)) return;

            var alivePlayers = frame.ResolveList<EntityRef>(frame.Global->AlivePlayers);

            foreach (var entity in alivePlayers)
            {
                if (PlayerView.PlayerTransforms.TryGetValue(entity, out var playerTransform))
                {
                    _inputManager.SetSoloMap(_inputManager.PlayerControls.DeathCamera);
                    _deathCamera.Follow = playerTransform;
                    break;
                }
                _currentPlayerIndex++;
            }
        }
    }

    private void Update()
    {
        if (_deathCamera.Follow == null)
        {
            SwitchToNextPlayer();
        }

        if (_inputManager.PlayerControls.DeathCamera.SwitchToNext.WasPressedThisFrame())
        {
            SwitchToNextPlayer();
        }
        else if (_inputManager.PlayerControls.DeathCamera.SwitchToPrevious.WasPressedThisFrame())
        {
            SwitchToPreviousPlayer();
        }
    }

    private void SwitchToNextPlayer()
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (frame == null) return;

        var alivePlayers = frame.ResolveList<EntityRef>(frame.Global->AlivePlayers);

        _currentPlayerIndex = _currentPlayerIndex >= alivePlayers.Count - 1
            ? 0
            : _currentPlayerIndex + 1;

        for (int i = _currentPlayerIndex; i < alivePlayers.Count; i++)
        {
            if (PlayerView.PlayerTransforms.TryGetValue(alivePlayers[i], out var playerTransform))
            {
                _deathCamera.Follow = playerTransform;
                _currentPlayerIndex = i;
                break;
            }
        }
    }

    private void SwitchToPreviousPlayer()
    {
        var frame = QuantumRunner.Default.Game.Frames.Verified;
        if (frame == null) return;

        var alivePlayers = frame.ResolveList<EntityRef>(frame.Global->AlivePlayers);

        _currentPlayerIndex = _currentPlayerIndex <= 0
           ? alivePlayers.Count - 1
           : _currentPlayerIndex - 1;

        for (int i = _currentPlayerIndex; i < alivePlayers.Count; i--)
        {
            if (PlayerView.PlayerTransforms.TryGetValue(alivePlayers[i], out var playerTransform))
            {
                _deathCamera.Follow = playerTransform;
                _currentPlayerIndex = i;
                break;
            }
        }
    }
}
