using UnityEngine;
using UnityEngine.UI;

public class DefaultToggleSetter : MonoBehaviour
{
    [SerializeField] private Toggle _toggle;

    private void OnEnable()
    {
        Invoke("EnableDefaultToggle", .001f);
    }

    private void EnableDefaultToggle()
    {
        _toggle.isOn = true;
    }
}
