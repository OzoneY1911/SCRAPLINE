using UnityEngine;

public class CustomRotation : MonoBehaviour
{
    [Header("Axis Selection")]
    [SerializeField] private bool _rotateX, _rotateY, _rotateZ;

    [Header("Speed")]
    [SerializeField] private float _rotationSpeed;

    private void Update()
    {
        var x = _rotateX ? 1f : 0f;
        var y = _rotateY ? 1f : 0f;
        var z = _rotateZ ? 1f : 0f;

        var rotation = new Vector3(x, y, z);

        transform.Rotate(rotation * _rotationSpeed * Time.deltaTime);
    }
}
