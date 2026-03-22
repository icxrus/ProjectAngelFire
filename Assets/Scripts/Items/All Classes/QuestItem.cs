using UnityEngine;

[CreateAssetMenu(fileName = "Quest Item", menuName = "Items/Quest Item")]
public class QuestItem : Item
{
    [Header("Quest Item Properties")]
    [SerializeField] private string questID;

    public override Item GetItem()
    {
        return this;
    }
}
