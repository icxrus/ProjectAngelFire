using UnityEngine;

[System.Serializable]
public class InventoryDataClass
{
    [SerializeField] private Item item;
    [SerializeField] private int quantity;

    public InventoryDataClass(Item _item, int _quantity)
    {
        this.item = _item;
        this.quantity = _quantity;
    }

    public Item GetItem() { return item; }
    public int GetQuantity() { return quantity; }
    public void AddQuantity(int amount) { quantity += amount; }
}
