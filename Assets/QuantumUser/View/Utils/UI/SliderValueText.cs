using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueText : MonoBehaviour
{
    private Slider _slider;
    [SerializeField] private TextMeshProUGUI _valueText;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnEnable()
    {
        SetValueText(_slider.value);
    }

    private void OnSliderValueChanged(float value)
    {
        SetValueText(value);
    }

    private void SetValueText(float value)
    {
        _valueText.text = Mathf.RoundToInt(value * 100).ToString();
    }
}
