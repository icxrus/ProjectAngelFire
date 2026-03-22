using UnityEngine;

[CreateAssetMenu(fileName = "Refined Material", menuName = "Items/Refined Material")]
public class RefinedMaterial : Item
{
    public override Item GetItem()
    {
        return this;
    }
}
