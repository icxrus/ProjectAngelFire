using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryUISlotParent;
    private GameObject[] inventoryUISlots = new GameObject[24];

    private void Awake()
    {
        InventoryManager.OnInventoryChanged += UpdateInventoryUI;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= UpdateInventoryUI;
    }

    private void Start()
    {
        for (int i = 0; i < inventoryUISlotParent.transform.childCount; i++)
            inventoryUISlots[i] = inventoryUISlotParent.transform.GetChild(i).gameObject;
    }

    private void UpdateInventoryUI(InventoryDataClass[] inventoryData)
    {
        for (int i = 0; i < inventoryUISlots.Length; i++)
        {
            if (i >= inventoryData.Length) break;

            InventoryDataClass slotData = inventoryData[i];

            Image iconImage = inventoryUISlots[i].transform.GetChild(0).GetComponent<Image>();
            TMP_Text quantityText = inventoryUISlots[i].transform.GetChild(1).GetComponent<TMP_Text>();

            if (slotData != null && slotData.GetItem() != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = slotData.GetItem().itemIcon;

                // Only show text if stackable and quantity > 1
                bool showQuantity = slotData.GetItem().isStackable && slotData.GetQuantity() > 1;
                quantityText.text = showQuantity ? slotData.GetQuantity().ToString() : "";
            }
            else
            {
                // Slot is empty
                iconImage.enabled = false;
                iconImage.sprite = null;
                quantityText.text = "";
            }
        }
    }
}
