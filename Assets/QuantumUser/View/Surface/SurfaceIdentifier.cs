using UnityEngine;

public class SurfaceIdentifier : MonoBehaviour
{
    [SerializeField] private SurfaceType _surfaceType;

    public SurfaceType SurfaceType => _surfaceType;
}
