using UnityEngine;

[CreateAssetMenu(fileName = "Misc", menuName = "Items/Misc")]
public class Misc : Item
{
    public override Item GetItem()
    {
        return this;
    }
}
