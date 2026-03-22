using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour, ISaveable
{
    [SerializeField] private InventoryDataClass[] inventory = new InventoryDataClass[24];

    public delegate void InventoryChanged(InventoryDataClass[] currentInventory);
    public static event InventoryChanged OnInventoryChanged;

    public string SaveID => "inventory";

    public bool AddItemToInventory(Item item, int quantity)
    {
        int remainingQuantity = quantity;

        //Try to fill existing stacks first (if stackable)
        if (item.isStackable)
        {
            foreach (var slot in inventory)
            {
                if (slot != null && slot.GetItem().itemID == item.itemID)
                {
                    int roomInStack = item.maxStackSize - slot.GetQuantity();
                    if (roomInStack > 0)
                    {
                        int amountToAdd = Mathf.Min(remainingQuantity, roomInStack);
                        slot.AddQuantity(amountToAdd);
                        remainingQuantity -= amountToAdd;
                    }
                }
                if (remainingQuantity <= 0) break;
            }
        }

        // If we still have items, find empty slots
        if (remainingQuantity > 0)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i] == null)
                {
                    // If not stackable, we only put 1 in this slot and keep looping
                    // If stackable, we put as much as possible (up to maxStackSize)
                    int amountInNewSlot = item.isStackable ? Mathf.Min(remainingQuantity, item.maxStackSize) : 1;

                    inventory[i] = new InventoryDataClass(item, amountInNewSlot);
                    remainingQuantity -= amountInNewSlot;
                }
                else
                    break;
                if (remainingQuantity <= 0) break;
            }
        }

        // Final check
        if (remainingQuantity < quantity)
        {
            OnInventoryChanged?.Invoke(inventory);
        }

        if (remainingQuantity > 0)
        {
            Debug.LogWarning($"Inventory full! {remainingQuantity} of {item.itemName} could not be added.");
            return false;
        }

        return true;
    }

    public bool RemoveItemFromInventory(Item item, int quantity)
    {
        //Calculate the total amount of this item currently in inventory
        int totalAvailable = 0;
        foreach (var slot in inventory)
        {
            if (slot != null && slot.GetItem().itemID == item.itemID)
            {
                totalAvailable += slot.GetQuantity();
            }
        }

        // Clamp the quantity to remove to the total available, so we don't try to remove more than we have.
        int remainingToRemove = Mathf.Min(quantity, totalAvailable);

        // If we have 0 of the item, we can just stop here.
        if (remainingToRemove <= 0) return false;

        // Removal pass
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null || inventory[i].GetItem().itemID != item.itemID)
                continue;

            int currentQty = inventory[i].GetQuantity();

            if (currentQty <= remainingToRemove)
            {
                remainingToRemove -= currentQty;
                inventory[i] = null;
            }
            else
            {
                inventory[i].AddQuantity(-remainingToRemove);
                remainingToRemove = 0;
            }

            if (remainingToRemove <= 0) break;
        }

        OnInventoryChanged?.Invoke(inventory);
        return true;
    }

    public InventoryDataClass[] GetInventory()
    {
        return inventory;
    }

    #region Save System Methods
    public object FetchSaveData()
    {
        return new InventorySaveData
        {
            inventoryItems = this.inventory
        };
    }

    public void LoadSaveData(string jsonData)
    {
        InventorySaveData data = JsonUtility.FromJson<InventorySaveData>(jsonData);
        this.inventory = data.inventoryItems;
        OnInventoryChanged?.Invoke(inventory);
    }
}

[System.Serializable]
public class InventorySaveData
{
    public InventoryDataClass[] inventoryItems;
}
#endregion