using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Items/Armor")]
public class Armor : Item
{
    [Header("Armor Specific")]
    [SerializeField] private ArmorType armorType;

    public override Item GetItem()
    {
        return this;
    }
}

public enum ArmorType
{
    Head,
    Cape,
    Chest,
    Legs,
    Feet
}
