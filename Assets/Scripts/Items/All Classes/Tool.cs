using UnityEngine;

[CreateAssetMenu(fileName = "Tool", menuName = "Items/Tool")]
public class Tool : Item
{
    [Header("Tool Properties")]
    [SerializeField] private ToolType toolType;

    public override Item GetItem()
    {
        return this;
    }
}

public enum ToolType
{
    Pickaxe,
    Axe,
    Shovel
}
