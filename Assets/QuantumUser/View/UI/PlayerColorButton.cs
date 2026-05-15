using Photon.Deterministic;
using Quantum;
using UnityEngine;
using UnityEngine.UI;

public class PlayerColorButton : MonoBehaviour
{
    [SerializeField] private Color _color;

    private FPVector3 _colorRGB;

    private void Awake()
    {
        _colorRGB = new FPVector3(
            FP.FromFloat_UNSAFE(_color.r),
            FP.FromFloat_UNSAFE(_color.g),
            FP.FromFloat_UNSAFE(_color.b)
            );
    }

    private void Start()
    {
        if (_colorRGB == PlayerCustomizationUtils.GetPlayerColorRGB())
        {
            GetComponent<Toggle>().isOn = true;
        }
    }

    public void SetColor()
    {
        CustomizationPreviewManager.Instance.SetColor(_color);
        PlayerCustomizationUtils.SavePlayerColor(_color);

        if (QuantumRunner.Default == null) return;

        var command = new CommandSetPlayerColor()
        {
            ColorRGB = _colorRGB
        };
        QuantumRunner.Default.Game.AddCommand(command);
    }
}
