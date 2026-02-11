using UnityEngine;

namespace Quantum
{
    public class SceneContext : MonoBehaviour, IQuantumViewContext
    {
        public InputHandler InputHandler;

        public PlayerRef LocalPlayer;
        public EntityRef LocalPlayerEntity;
        public PlayerView LocalPlayerView;
    }
}