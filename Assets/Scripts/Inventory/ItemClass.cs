using UnityEditor;
using UnityEngine;

public abstract class ItemClass : ScriptableObject
{
    //Base data for all items, such as ID, name, and icon. This is the base class for all items in the inventory system.
    [Header("Base Item Data")]
    public string itemID;
    public string itemName;
    public string description;
    public Sprite itemIcon;

    public int value;
    public Rarity rarity;

    public bool isStackable = true;
    public int maxStackSize = 16;

    [Header("Finalize Item Data")]
    [Tooltip("DANGEROUS!!! Set to true to lock in item Name and generate Unique ID. Unchecking will reset ID. DANGEROUS!!!")]
    public bool lockItem;

    public enum Rarity 
    { 
        Common, 
        Uncommon, 
        Rare, 
        Epic, 
        Legendary 
    }

    public abstract ItemClass GetItem();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (lockItem && string.IsNullOrEmpty(itemID) && !string.IsNullOrEmpty(itemName))
        {
            itemID = $"{itemName}_{System.Guid.NewGuid()}";
        }

        //Reset itemID if lock is removed to prevent confusion and ensure unique IDs are not reused. WILL BREAK EXISTING REFERENCES IF UNLOCKED, USE WITH CAUTION.
        if (!lockItem && !string.IsNullOrEmpty(itemID))
        {
            itemID = string.Empty;
        }
    }
#endif

}
