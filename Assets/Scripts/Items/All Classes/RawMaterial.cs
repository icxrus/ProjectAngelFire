using UnityEngine;

[CreateAssetMenu(fileName = "Raw Material", menuName = "Items/Raw Material")]
public class RawMaterial : Item
{
    public override Item GetItem()
    {
        return this;
    }
}
