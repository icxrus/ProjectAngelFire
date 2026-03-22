using UnityEngine;

[CreateAssetMenu(fileName = "Note", menuName = "Items/Note Item")]
public class Notes : Item
{
    [Header("Note Details")]
    [SerializeField] private string noteTitle;
    [TextArea(3, 10)]
    [SerializeField] private string noteContent;
    

    public override Item GetItem()
    {
        return this;
    }
}
