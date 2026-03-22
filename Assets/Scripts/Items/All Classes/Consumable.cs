using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "Items/Consumable")]
public class Consumable : Item
{
    [Header("Consumable Properties")]
    [SerializeField] private int healthRestoreAmount;
    [SerializeField] private int hungerRestoreAmount;

    public override Item GetItem()
    {
        return this;
    }
}
