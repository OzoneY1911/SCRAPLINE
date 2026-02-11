using Quantum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuotaZoneProgressBar : MonoBehaviour
{
    public TextMeshProUGUI TextTMP;
    public Image BarImage;

    [HideInInspector]
    public ResourceType Type;

    [HideInInspector]
    public float CurrentValue;

    [HideInInspector]
    public float MaxValue;
}
