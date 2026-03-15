using UnityEngine;

[System.Serializable]
public class InventoryDataClass
{
    [SerializeField] private ItemClass item;
    [SerializeField] private int quantity;

    public InventoryDataClass(ItemClass _item, int _quantity)
    {
        this.item = _item;
        this.quantity = _quantity;
    }

    public ItemClass GetItem() { return item; }
    public int GetQuantity() { return quantity; }
    public void AddQuantity(int amount) { quantity += amount; }
}
