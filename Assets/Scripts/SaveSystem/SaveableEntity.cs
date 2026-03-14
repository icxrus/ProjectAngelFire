using UnityEngine;

public abstract class SaveableEntity : MonoBehaviour, ISaveable
{
    [SerializeField] private string _saveID;
    public string SaveID => _saveID;

    // Automatic ID Gen
    protected virtual void Reset()
    {
        if (string.IsNullOrEmpty(_saveID))
        {
            _saveID = $"{gameObject.name}_{System.Guid.NewGuid()}";
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }
    }

    public abstract object FetchSaveData();
    public abstract void LoadSaveData(string json);
}