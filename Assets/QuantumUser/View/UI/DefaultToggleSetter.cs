using UnityEngine;
using UnityEngine.UI;

public class DefaultToggleSetter : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;

    private void OnEnable()
    {
        _toggle.isOn = true;
    }
}
