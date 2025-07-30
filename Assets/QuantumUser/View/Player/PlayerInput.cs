using Photon.Deterministic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Quantum
{
    public sealed class PlayerInput : MonoBehaviour
    {
        public static float LookSensitivity = 3f;

        [SerializeField]
        private QuantumEntityViewUpdater _entityViewUpdater;

        private Quantum.Input _accumulatedInput;
        private bool _resetAccumulatedInput;
        private int _lastAccumulateFrame;

        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        private void Update()
        {
            AccumulateInput();
        }

        private void AccumulateInput()
        {
            if (_lastAccumulateFrame == Time.frameCount)
                return;

            _lastAccumulateFrame = Time.frameCount;

            if (_resetAccumulatedInput)
            {
                _resetAccumulatedInput = false;
                _accumulatedInput = default;
            }
                
            ProcessStandaloneInput();
        }

        private void ProcessStandaloneInput()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            if (keyboard == null || mouse == null)
                return;

            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            // Process keyboard input

            Vector2 moveDirection = Vector2.zero;

            if (keyboard.wKey.isPressed) { moveDirection += Vector2.up; }
            if (keyboard.sKey.isPressed) { moveDirection += Vector2.down; }
            if (keyboard.aKey.isPressed) { moveDirection += Vector2.left; }
            if (keyboard.dKey.isPressed) { moveDirection += Vector2.right; }

            _accumulatedInput.MoveDirection = moveDirection.normalized.ToFPVector2();

            // Process mouse input

            Vector2 mouseDelta = mouse.delta.ReadValue();

            Vector2 lookRotationDelta = new Vector2(-mouseDelta.y, mouseDelta.x);
            lookRotationDelta *= LookSensitivity / 60f;
            _accumulatedInput.LookRotationDelta += lookRotationDelta.ToFPVector2();
        }

        public void PollInput(CallbackPollInput callback)
        {
            AccumulateInput();

            _accumulatedInput.InterpolationOffset = (byte)Mathf.Clamp(callback.Frame - _entityViewUpdater.SnapshotInterpolation.CurrentFrom, 0, 255);
            _accumulatedInput.InterpolationAlpha = _entityViewUpdater.SnapshotInterpolation.Alpha.ToFP();

            callback.SetInput(_accumulatedInput, DeterministicInputFlags.Repeatable);

            _resetAccumulatedInput = true;
            _accumulatedInput.LookRotationDelta = default;
        }
    }
}
