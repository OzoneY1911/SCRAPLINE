using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public sealed class InputHandler : MonoBehaviour
    {
        public static float LookSensitivity = 3f;

        [SerializeField] private QuantumEntityViewUpdater _entityViewUpdater;

        [SerializeField] private GameObject _playerCameraObject;

        private PlayerControls _playerControls;

        private Quantum.Input _accumulatedInput;
        private bool _resetAccumulatedInput;
        private int _lastAccumulateFrame;

        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        private void Start()
        {
            _playerControls = GetComponent<InputManager>().PlayerControls;
        }

        private void Update()
        {
            AccumulateInput();

            _accumulatedInput.CameraPosition = _playerCameraObject.transform.position.ToFPVector3();
            _accumulatedInput.CameraForward = _playerCameraObject.transform.forward.ToFPVector3();
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

            ProcessInput();
        }

        private void ProcessInput()
        {
            // Process keyboard input

            _accumulatedInput.MoveDirection = _playerControls.Main.Move.ReadValue<Vector2>().normalized.ToFPVector2();

            _accumulatedInput.Jump = _playerControls.Main.Jump.IsPressed();
            _accumulatedInput.Run = _playerControls.Main.Run.IsPressed();
            _accumulatedInput.Crouch = _playerControls.Main.Crouch.IsPressed();
            _accumulatedInput.Interact = _playerControls.Main.Interact.IsPressed();
            _accumulatedInput.SecondaryAction = _playerControls.Main.SecondaryAction.IsPressed();

            _accumulatedInput.SelectInventorySlot = _playerControls.Main.SelectInventorySlot.IsPressed();
            if (_accumulatedInput.SelectInventorySlot)
            {
                _accumulatedInput.SelectedInventorySlotIndex = (SlotIndex)_playerControls.Main.SelectInventorySlot.ReadValue<float>();
            }

            _accumulatedInput.CollectValuable = _playerControls.Main.CollectValuable.IsPressed();
            _accumulatedInput.DropValuable = _playerControls.Main.DropValuable.IsPressed();

            // Process mouse input

            Vector2 mouseDelta = _playerControls.Main.Look.ReadValue<Vector2>();

            Vector2 lookRotationDelta = new Vector2(-mouseDelta.y, mouseDelta.x);
            lookRotationDelta *= LookSensitivity / 60f;
            _accumulatedInput.LookRotationDelta += lookRotationDelta.ToFPVector2();

            _accumulatedInput.ScrollDelta += _playerControls.Main.Scroll.ReadValue<Vector2>().y.ToFP();
            _accumulatedInput.ScrollDelta = FPMath.Clamp(_accumulatedInput.ScrollDelta, -FP._1, FP._1);
        }

        public void PollInput(CallbackPollInput callback)
        {
            //AccumulateInput();
            ProcessInput();

            _accumulatedInput.InterpolationOffset = (byte)Mathf.Clamp(callback.Frame - _entityViewUpdater.SnapshotInterpolation.CurrentFrom, 0, 255);
            _accumulatedInput.InterpolationAlpha = _entityViewUpdater.SnapshotInterpolation.Alpha.ToFP();

            callback.SetInput(_accumulatedInput, DeterministicInputFlags.Repeatable);

            //_resetAccumulatedInput = true;
            _accumulatedInput.LookRotationDelta = default;
            _accumulatedInput.ScrollDelta = FP._0;
        }
    }
}
