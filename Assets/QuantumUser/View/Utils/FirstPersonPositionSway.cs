using UnityEngine;

namespace Quantum
{

    public unsafe class FirstPersonPositionSway : QuantumEntityViewComponent
    {
        [Header("Position Sway Settings")]
        [SerializeField] private float _swaySpeed = 3f;
        [SerializeField] private float _swayStrength = 1f;
        [SerializeField] private float _maxOffset = 0.015f;

        private Vector3 _initialLocalPosition;

        void Update()
        {
            if (VerifiedFrame.TryGet(EntityRef, out Player player) == false) return;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            if (!isLocal) return;

            Frame frame = PredictedFrame;
            if (frame.Exists(EntityRef) == false) return;

            Vector2 moveDirection = frame.GetPlayerInput(player.PlayerRef)->MoveDirection.ToUnityVector2();

            // Calculate offset (x = strafe, z = forward/back)
            float offsetX = Mathf.Clamp(-moveDirection.x * _swayStrength, -_maxOffset, _maxOffset);
            float offsetZ = Mathf.Clamp(-moveDirection.y * _swayStrength, -_maxOffset, _maxOffset);

            Vector3 targetPosition = _initialLocalPosition + new Vector3(offsetX, 0f, offsetZ);

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, _swaySpeed * Time.deltaTime);
        }
    }
}