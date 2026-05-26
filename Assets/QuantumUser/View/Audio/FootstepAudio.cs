using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    [SerializeField] private EventReference _footstepEvent;

    [SerializeField] private float _rayDistance = .5f;

    public void PlayFootstep()
    {
        SurfaceType surfaceType = GetSurface();

        EventInstance instance = RuntimeManager.CreateInstance(_footstepEvent);

        instance.setParameterByNameWithLabel("Surface", surfaceType.ToString());
        RuntimeManager.AttachInstanceToGameObject(instance, gameObject);

        instance.start();
        instance.release();
    }

    private SurfaceType GetSurface()
    {
        if (Physics.Raycast(
            transform.position,
            Vector3.down,
            out RaycastHit hit,
            _rayDistance))
        {
            SurfaceIdentifier surface =
                hit.collider.GetComponent<SurfaceIdentifier>();
            if (surface != null)
            {
                return surface.SurfaceType;
            }
        }

        return SurfaceType.Metal;
    }
}