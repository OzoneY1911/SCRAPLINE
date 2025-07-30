using UnityEngine;
using Photon.Deterministic;
using Unity.Cinemachine;

namespace Quantum
{
    public class PlayerView : QuantumEntityViewComponent<SceneContext>
    {
        [Header("Setup")]
        public Transform CameraHandle;

        const float _cameraSmooth = 0.25f;

        private float _smoothPitch;
        private float _smoothPitchVelocity;

        public override void OnActivate(Frame frame)
        {
            if (frame.TryGet(EntityRef, out Player player) == false)
                return;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            if (isLocal)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                ViewContext.LocalPlayerView = this;
                ViewContext.LocalPlayer = player.PlayerRef;
                ViewContext.LocalPlayerEntity = EntityRef;

                // Local player is always predicted.
                EntityView.InterpolationMode = QuantumEntityViewInterpolationMode.Prediction;
            }
            else
            {
                // Virtual cameras are enabled only for local player.
                var cinemachineCameras = GetComponentsInChildren<CinemachineCamera>(true);
                for (int i = 0; i < cinemachineCameras.Length; i++)
                {
                    cinemachineCameras[i].enabled = false;
                }

                // Other player views are snapshot interpolated.
                EntityView.InterpolationMode = QuantumEntityViewInterpolationMode.SnapshotInterpolation;
            }
        }

        public override void OnDeactivate()
        {
            if (ViewContext.LocalPlayerView == this)
            {
                ViewContext.LocalPlayerView = null;
                ViewContext.LocalPlayerEntity = EntityRef.None;
            }
        }

        public override void OnLateUpdateView()
        {
            Frame verifiedFrame = VerifiedFrame;
            if (verifiedFrame.TryGet(EntityRef, out Player player) == false)
                return;

            Frame predictedFrame = PredictedFrame;
            if (predictedFrame.Exists(EntityRef) == false)
                return;

            float lookYaw = player.LookYaw.AsFloat;
            float lookPitch = player.LookPitch.AsFloat;

            bool isLocal = Game.PlayerIsLocal(player.PlayerRef);
            if (isLocal)
            {
                if (PredictedPreviousFrame.TryGet<Player>(EntityRef, out Player previousPlayer))
                {
                    lookYaw = Mathf.LerpAngle(previousPlayer.LookYaw.AsFloat, previousPlayer.LookYaw.AsFloat, EntityView.Game.InterpolationFactor);
                    lookPitch = Mathf.LerpAngle(previousPlayer.LookPitch.AsFloat, previousPlayer.LookPitch.AsFloat, EntityView.Game.InterpolationFactor);
                }

                transform.rotation = Quaternion.Euler(0.0f, lookYaw, 0.0f);
            }
            else
            {
                if (verifiedFrame.TryGet<Player>(EntityRef, out Player verifiedPlayer))
                {
                    _smoothPitch = Mathf.SmoothDamp(_smoothPitch, verifiedPlayer.LookPitch.AsFloat, ref _smoothPitchVelocity, 0.1f);
                    lookPitch = _smoothPitch;
                }
            }

            lookPitch = Mathf.Clamp(lookPitch, -89.0f, 89.0f);

            CameraHandle.localRotation = Quaternion.Euler(lookPitch, 0.0f, 0.0f);
        }

        private Vector3 GetAnimationMoveVelocity(KCC kcc)
        {
            if (kcc.RealSpeed < FP._0_01)
                return default;

            var velocity = kcc.RealVelocity;

            // We only care about X an Z directions.
            velocity.Y = 0;

            if (velocity.SqrMagnitude > 1)
            {
                velocity = velocity.Normalized;
            }

            // Transform velocity vector to local space.
            return transform.InverseTransformVector(velocity.ToUnityVector3());
        }
    }
}
