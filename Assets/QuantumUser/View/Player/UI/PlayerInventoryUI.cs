using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerInventoryUI : MonoBehaviour
{
    [SerializeField] private List<Image> _slotImages;

    public void SetSelectedSlot(int slotIndex)
    {
        for (int i = 0; i < _slotImages.Count; i++)
        {
            _slotImages[i].enabled = (i == slotIndex - 1);
        }
    }
}
