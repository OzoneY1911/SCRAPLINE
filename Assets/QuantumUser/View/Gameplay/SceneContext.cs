using UnityEngine;

namespace Quantum
{
    public class SceneContext : MonoBehaviour, IQuantumViewContext
    {
        public PlayerInput PlayerInput;

        public PlayerRef LocalPlayer;
        public EntityRef LocalPlayerEntity;
        public PlayerView LocalPlayerView;
    }
}