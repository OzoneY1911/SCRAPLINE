using Photon.Deterministic;
using Quantum;
using UnityEngine;

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

    public void SetColor()
    {
        CustomizationPreviewManager.Instance.SetColor(_color);

        if (QuantumRunner.Default == null) return;

        var command = new CommandSetPlayerColor()
        {
            ColorRGB = _colorRGB
        };
        QuantumRunner.Default.Game.SendCommand(command);
    }
}
