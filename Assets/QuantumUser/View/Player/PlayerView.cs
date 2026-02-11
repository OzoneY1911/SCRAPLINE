using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

namespace Quantum
{
    public unsafe class PlayerView : QuantumEntityViewComponent<SceneContext>
    {
        [Header("Camera Handle")]
        public Transform CameraHandle;

        private float _smoothPitch;
        private float _smoothPitchVelocity;

        private bool _isLocal;

        public static readonly Dictionary<EntityRef, Transform> PlayerTransforms = new();

        public override void OnActivate(Frame frame)
        {
            if (!frame.Unsafe.TryGetPointer<Player>(EntityRef, out Player* player)) return;
            
            PlayerTransforms[EntityRef] = transform;

            _isLocal = Game.PlayerIsLocal(player->PlayerRef);

            if (_isLocal)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                ViewContext.LocalPlayerView = this;
                ViewContext.LocalPlayer = player->PlayerRef;
                ViewContext.LocalPlayerEntity = EntityRef;

                // Local player is always predicted.
                EntityView.InterpolationMode = QuantumEntityViewInterpolationMode.Prediction;

                var playerRenderers = GetComponentsInChildren<Renderer>(true);

                foreach (var renderer in playerRenderers)
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
            PlayerTransforms.Remove(EntityRef);

            if (ViewContext.LocalPlayerView == this)
            {
                ViewContext.LocalPlayerView = null;
                ViewContext.LocalPlayerEntity = EntityRef.None;
            }
        }

        public override void OnLateUpdateView()
        {
            var frame = _isLocal ? PredictedFrame : VerifiedFrame;

            if (frame == null) return;
            if (!frame.Exists(EntityRef)) return;

            if (!frame.TryGet(EntityRef, out Player player))
                return;

            var movement = frame.Get<PlayerMovement>(EntityRef);

            float lookYaw = player.LookYaw.AsFloat;
            float lookPitch = player.LookPitch.AsFloat;

            if (_isLocal)
            {
                if (PredictedPreviousFrame.TryGet<Player>(EntityRef, out Player previousPlayer))
                {
                    lookYaw = Mathf.LerpAngle(previousPlayer.LookYaw.AsFloat, player.LookYaw.AsFloat, EntityView.Game.InterpolationFactor);
                    lookPitch = Mathf.LerpAngle(previousPlayer.LookPitch.AsFloat, player.LookPitch.AsFloat, EntityView.Game.InterpolationFactor);
                }

                //transform.rotation = Quaternion.Euler(0.0f, lookYaw, 0.0f);
            }
            else
            {
                if (frame.TryGet<Player>(EntityRef, out Player verifiedPlayer))
                {
                    _smoothPitch = Mathf.SmoothDamp(_smoothPitch, verifiedPlayer.LookPitch.AsFloat, ref _smoothPitchVelocity, 0.1f);
                    lookPitch = _smoothPitch;
                }
            }

            CameraHandle.localRotation = Quaternion.Euler(lookPitch, 0.0f, 0.0f);

            //Vector3 targetScale = movement.IsCrouching ? new Vector3(1.0f, 0.5f, 1.0f) : Vector3.one;

            //transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * movement.CrouchLerpSpeed.AsFloat);

            Vector3 targetLocalPosition = movement.IsCrouching
                ? new Vector3(0f, movement.CameraCrouchHeight.AsFloat, 0f)
                : new Vector3(0f, movement.CameraStandHeight.AsFloat, 0f);

            CameraHandle.localPosition = Vector3.Lerp(CameraHandle.localPosition, targetLocalPosition, Time.deltaTime * movement.CrouchLerpSpeed.AsFloat);
        }
    }
}
