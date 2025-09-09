using Quantum;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public unsafe class DeathCameraController : MonoBehaviour
{
    CinemachineCamera _deathCamera;

    private int _currentPlayerIndex;

    private void Awake()
    {
        _deathCamera = GetComponent<CinemachineCamera>();
    }

    private void OnEnable()
    {
        QuantumEvent.Subscribe<EventEntityDeath>(this, OnEventEntityDeath);
    }

    private void OnEventEntityDeath(EventEntityDeath e)
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        var frame = game.Frames.Verified;
        if (frame == null) return;

        if (frame.Has<Player>(e.Entity))
        {
            var alivePlayers = frame.ResolveList<EntityRef>(frame.Global->AlivePlayers);

            foreach (var entity in alivePlayers)
            {
                if (PlayerView.PlayerTransforms.TryGetValue(entity, out var playerTransform))
                {
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

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SwitchToNextPlayer();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            SwitchToPreviousPlayer();
        }
    }

    private void SwitchToNextPlayer()
    {
        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        var frame = game.Frames.Verified;
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
        var game = QuantumRunner.Default.Game;
        if (game == null) return;

        var frame = game.Frames.Verified;
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
