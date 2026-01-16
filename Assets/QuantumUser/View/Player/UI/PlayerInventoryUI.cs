using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private List<Image> _slotImages;
    [SerializeField] private List<TextMeshProUGUI> _slotValuableNames;

    private void Awake()
    {
        foreach (var slotValuableName in _slotValuableNames)
        {
            slotValuableName.text = "";
        }
    }

    public void SetSelectedSlot(byte slotIndex)
    {
        for (int i = 0; i < _slotImages.Count; i++)
        {
            _slotImages[i].enabled = (i == slotIndex);
        }
    }

    public void SetSlotValuableName(byte slotIndex, string valuableName)
    {
        _slotValuableNames[slotIndex].text = valuableName;
    }
}
