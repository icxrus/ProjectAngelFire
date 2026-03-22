using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Items/Weapon")]
public class Weapon : Item
{
    [Header("Weapon Specific")]
    [SerializeField] private WeaponType weaponType;

    public override Item GetItem()
    {
        return this;
    }
}

public enum WeaponType
{
    Sword
}
