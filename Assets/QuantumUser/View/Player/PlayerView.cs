using UnityEngine;
using Unity.Cinemachine;

namespace Quantum
{
    public class PlayerView : QuantumEntityViewComponent<SceneContext>
    {
        [Header("Local Renderers")]
        public Renderer[] PlayerRenderers;

        [Header("Camera Handle")]
        public Transform CameraHandle;

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

                foreach (var renderer in PlayerRenderers)
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
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

            Vector3 targetScale = player.IsCrouching ? new Vector3(1.0f, 0.5f, 1.0f) : Vector3.one;

            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * player.CrouchLerpSpeed.AsFloat);
        }
    }
}
