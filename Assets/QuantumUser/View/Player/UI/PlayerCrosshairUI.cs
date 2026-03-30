using UnityEngine;

namespace Quantum
{
    public unsafe class PlayerCrosshairUI : MonoBehaviour
    {
        [SerializeField] private Animator _crosshairAnimator;

        private void Update()
        {
            var frame = QuantumRunner.Default.Game.Frames.Verified;

            if (frame == null) return;

            var localPlayers = QuantumRunner.Default.Game.GetLocalPlayers();

            if (localPlayers.Count == 0) return;

            var activePlayers = frame.ResolveDictionary<PlayerRef, EntityRef>(frame.Global->ActivePlayers);

            if (activePlayers.TryGetValue(localPlayers[0], out var localPlayerEntity))
            {
                if (frame.Unsafe.TryGetPointer(localPlayerEntity, out PlayerDragging* dragging))
                {
                    _crosshairAnimator.SetBool("IsDragging", dragging->IsDragging);
                }
            }
        }
    }
}
