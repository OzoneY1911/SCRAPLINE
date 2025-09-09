using Quantum;
using Unity.Cinemachine;
using UnityEngine;

public unsafe class DeathCameraController : MonoBehaviour
{
    CinemachineCamera _deathCamera;

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
            var alivePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->AlivePlayers);

            var randomAlivePlayerIndex = Random.Range(0, alivePlayers.Count);

            _deathCamera.Follow = PlayerView.PlayerTransforms[alivePlayers[randomAlivePlayerIndex]];
        }
    }
}
